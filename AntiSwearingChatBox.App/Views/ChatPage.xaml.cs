using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace AntiSwearingChatBox.App.Views
{
    /// <summary>
    /// Interaction logic for ChatPage2.xaml
    /// </summary>
    public partial class ChatPage : Page
    {
        private HubConnection? _connection;
        private readonly IMessageHistoryService _messageHistoryService;
        private readonly IChatThreadService _chatThreadService;
        private readonly IUserService _userService;
        private readonly IThreadParticipantService _threadParticipantService;
        private readonly IAuthService _authService;
        private int _currentUserId;
        private User? _currentUser;
        
        public ChatPage()
        {
            InitializeComponent();

            // Get services from DI container
            var app = (App)Application.Current;
            _messageHistoryService = app.ServiceProvider.GetRequiredService<IMessageHistoryService>();
            _chatThreadService = app.ServiceProvider.GetRequiredService<IChatThreadService>();
            _userService = app.ServiceProvider.GetRequiredService<IUserService>();
            _threadParticipantService = app.ServiceProvider.GetRequiredService<IThreadParticipantService>();
            _authService = app.ServiceProvider.GetRequiredService<IAuthService>();
            
            // Initialize events
            InitializeEvents();
            
            // Load user
            LoadCurrentUser();
        }
        
        private void InitializeEvents()
        {
            this.Loaded += Page_Loaded;
        }
        
        private void LoadCurrentUser()
        {
            // Get current user from app
            var app = (App)Application.Current;
            if (app.CurrentUser != null)
            {
                _currentUser = app.CurrentUser;
                _currentUserId = app.CurrentUser.UserId;
                // Update UI with user info
                UpdateUserDisplay(app.CurrentUser.Username);
            }
            else
            {
                MessageBox.Show("Authentication error. Please log in again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Navigate back to login page
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.NavigateToLogin();
                }
                return;
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Load chat threads
            LoadChatThreads();
            
            // Initialize SignalR connection
            await InitializeConnection();
        }

        private void UpdateUserDisplay(string username)
        {
            // Get main window
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.Title = $"Chat - {username} - Anti-Swearing Chat Box";
            }
        }

        private void LoadChatThreads()
        {
            try
            {
                // Clear existing conversations
                ClearConversations();
                
                // Get all threads the user is a participant in
                var userThreads = _threadParticipantService.GetByUserId(_currentUserId);
                if (userThreads.Any())
                {
                    foreach (var participant in userThreads)
                    {
                        var thread = _chatThreadService.GetById(participant.ThreadId);
                        if (thread != null)
                        {
                            // Find last message
                            var lastMessage = _messageHistoryService.GetByThreadId(thread.ThreadId)
                                .OrderByDescending(m => m.CreatedAt)
                                .FirstOrDefault();
                            
                            // Add thread to conversation list
                            AddConversation(
                                thread.ThreadId.ToString(),
                                thread.Title,
                                lastMessage != null ? 
                                    (lastMessage.WasModified ? lastMessage.ModeratedMessage : lastMessage.OriginalMessage) : 
                                    "No messages yet",
                                lastMessage != null ? 
                                    lastMessage.CreatedAt.ToString("h:mm tt") : 
                                    thread.CreatedAt.ToString("h:mm tt")
                            );
                        }
                    }
                }
                else
                {
                    // Get general chat thread
                    var generalThread = _chatThreadService.GetAll().FirstOrDefault(t => !t.IsPrivate);
                    if (generalThread != null)
                    {
                        AddConversation(
                            generalThread.ThreadId.ToString(),
                            generalThread.Title,
                            "Welcome to the chat!",
                            generalThread.CreatedAt.ToString("h:mm tt")
                        );
                        
                        // Add user as participant to general chat
                        var participant = new ThreadParticipant
                        {
                            ThreadId = generalThread.ThreadId,
                            UserId = _currentUserId,
                            JoinedAt = DateTime.Now
                        };
                        
                        _threadParticipantService.Add(participant);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading chat threads: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ClearConversations()
        {
            // Clear conversations from the UI
            ConversationList.Conversations.Clear();
        }
        
        private void AddConversation(string id, string title, string lastMessage, string lastMessageTime)
        {
            // Add a conversation to the UI
            ConversationList.Conversations.Add(new Components.ConversationItemViewModel
            {
                Id = id,
                Title = title,
                LastMessage = lastMessage,
                LastMessageTime = lastMessageTime
            });
        }
        
        private async Task InitializeConnection()
        {
            try
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl("https://localhost:7042/chatHub")
                    .WithAutomaticReconnect()
                    .Build();

                // Register handlers for SignalR events
                _connection.On<string, string>("ReceiveMessage", (user, message) =>
                {
                    // Messages for current thread are already handled by ReceiveMessage with more parameters
                });

                _connection.On<string, string, int, DateTime>("ReceiveMessage", (user, message, userId, timestamp) =>
                {
                    // Only add message if it's for the current thread
                    if (ChatView.CurrentContact != null)
                    {
                        // Check if the message is for the current thread
                        if (int.TryParse(ChatView.CurrentContact.Id, out int currentThreadId) && 
                            currentThreadId == GetThreadIdForUser(userId))
                        {
                            bool isSent = userId == _currentUserId;
                            
                            Dispatcher.Invoke(() =>
                            {
                                ChatView.Messages.Add(new Components.MessageViewModel
                                {
                                    IsSent = isSent,
                                    Text = message,
                                    Timestamp = timestamp.ToString("h:mm tt"),
                                    Avatar = isSent ? "ME" : user.Substring(0, 2).ToUpper(),
                                    Background = (SolidColorBrush)(isSent ? 
                                        Application.Current.Resources["PrimaryBackgroundBrush"] : 
                                        Application.Current.Resources["SecondaryBackgroundBrush"]),
                                    BorderBrush = (SolidColorBrush)(isSent ? 
                                        Application.Current.Resources["PrimaryGreenBrush"] : 
                                        Application.Current.Resources["BorderBrush"])
                                });
                                
                                // Update conversation list
                                UpdateConversationLastMessage(currentThreadId.ToString(), message, timestamp.ToString("h:mm tt"));
                            });
                        }
                    }
                });

                _connection.On<string>("ReceiveModeratedMessage", (message) =>
                {
                    MessageBox.Show($"Your message was moderated: {message}", "Moderation Notice", MessageBoxButton.OK, MessageBoxImage.Information);
                });
                
                // Connect to SignalR hub
                await _connection.StartAsync();
                
                // Register user with hub
                if (_currentUser != null)
                {
                    await _connection.InvokeAsync("RegisterUser", _currentUser.UserId, _currentUser.Username);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to chat server: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void UpdateConversationLastMessage(string threadId, string message, string time)
        {
            var conversation = ConversationList.Conversations.FirstOrDefault(c => c.Id == threadId);
            if (conversation != null)
            {
                conversation.LastMessage = message;
                conversation.LastMessageTime = time;
                // Move this conversation to the top
                ConversationList.Conversations.Remove(conversation);
                ConversationList.Conversations.Insert(0, conversation);
            }
        }
        
        private int GetThreadIdForUser(int userId)
        {
            if (ChatView.CurrentContact != null && int.TryParse(ChatView.CurrentContact.Id, out int threadId))
            {
                return threadId;
            }
            
            // Try to find a private thread with this user
            var thread = _chatThreadService.GetPrivateThreadBetweenUsers(_currentUserId, userId);
            return thread?.ThreadId ?? -1;
        }
        
        private async void ChatView_MessageSent(object sender, string message)
        {
            if (_connection?.State != HubConnectionState.Connected)
            {
                MessageBox.Show("Not connected to chat server. Please try again later.", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (ChatView.CurrentContact == null)
            {
                MessageBox.Show("Please select a conversation first.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }
            
            try
            {
                // Parse thread ID
                if (!int.TryParse(ChatView.CurrentContact.Id, out int threadId))
                {
                    MessageBox.Show("Invalid thread ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Save message to history first
                var messageHistory = new MessageHistory
                {
                    ThreadId = threadId,
                    SenderId = _currentUserId,
                    OriginalMessage = message,
                    ModeratedMessage = message, // Initially, assume no moderation needed
                    WasModified = false,
                    CreatedAt = DateTime.Now
                };
                
                _messageHistoryService.Add(messageHistory);
                
                // Send message through SignalR
                await _connection.InvokeAsync("SendMessage", _currentUser?.Username ?? "User", message, threadId, _currentUserId);
                
                // Update UI - already handled by SignalR callback
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void ConversationList_ConversationSelected(object sender, string threadId)
        {
            if (string.IsNullOrEmpty(threadId) || !int.TryParse(threadId, out int threadIdInt))
            {
                return;
            }
            
            try
            {
                // Get the thread info
                var thread = _chatThreadService.GetById(threadIdInt);
                if (thread == null)
                {
                    MessageBox.Show("Thread not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Clear messages
                ChatView.Messages.Clear();
                
                // Set current contact
                ChatView.CurrentContact = new Components.ContactViewModel
                {
                    Id = threadId,
                    Name = thread.Title,
                    AvatarName = thread.IsPrivate ? thread.Title.Substring(0, 2).ToUpper() : "GC",
                    IsOnline = true
                };
                
                // Get messages for this thread
                var messages = _messageHistoryService.GetByThreadId(threadIdInt)
                    .OrderBy(m => m.CreatedAt)
                    .ToList();
                
                // Add messages to UI
                foreach (var message in messages)
                {
                    var user = _userService.GetById(message.SenderId);
                    bool isSent = message.SenderId == _currentUserId;
                    
                    ChatView.Messages.Add(new Components.MessageViewModel
                    {
                        IsSent = isSent,
                        Text = message.WasModified ? message.ModeratedMessage : message.OriginalMessage,
                        Timestamp = message.CreatedAt.ToString("h:mm tt"),
                        Avatar = isSent ? "ME" : (user?.Username?.Substring(0, 2).ToUpper() ?? "??"),
                        Background = (SolidColorBrush)(isSent ? 
                            Application.Current.Resources["PrimaryBackgroundBrush"] : 
                            Application.Current.Resources["SecondaryBackgroundBrush"]),
                        BorderBrush = (SolidColorBrush)(isSent ? 
                            Application.Current.Resources["PrimaryGreenBrush"] : 
                            Application.Current.Resources["BorderBrush"])
                    });
                }
                
                // Join the thread via SignalR
                if (_connection?.State == HubConnectionState.Connected)
                {
                    await _connection.InvokeAsync("JoinThread", threadId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading thread: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ConversationList_NewChatRequested(object sender, EventArgs e)
        {
            // Navigate to user selection page
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToUserSelection(_userService, _currentUserId, UserSelectionComplete, UserSelectionCancelled);
            }
        }
        
        private void UserSelectionCancelled(object sender, EventArgs e)
        {
            // User cancelled selection, nothing to do
        }
        
        private void UserSelectionComplete(object sender, UserSelectionEventArgs e)
        {
            try
            {
                if (e.IsGroupChat)
                {
                    // Group chat functionality not yet implemented
                    MessageBox.Show("Group chat functionality is not yet implemented.", "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                if (e.SelectedUser != null)
                {
                    // Check if thread already exists
                    var existingThread = _chatThreadService.GetPrivateThreadBetweenUsers(_currentUserId, e.SelectedUser.UserId);
                    if (existingThread != null)
                    {
                        // Select the existing thread
                        SelectChatThread(existingThread.ThreadId.ToString());
                        return;
                    }
                    
                    // Create new private thread
                    var newThread = new ChatThread
                    {
                        Title = e.SelectedUser.Username,
                        IsPrivate = true,
                        CreatedAt = DateTime.Now
                    };
                    
                    _chatThreadService.Add(newThread);
                    
                    // Add participants
                    var participant1 = new ThreadParticipant
                    {
                        ThreadId = newThread.ThreadId,
                        UserId = _currentUserId,
                        JoinedAt = DateTime.Now
                    };
                    
                    var participant2 = new ThreadParticipant
                    {
                        ThreadId = newThread.ThreadId,
                        UserId = e.SelectedUser.UserId,
                        JoinedAt = DateTime.Now
                    };
                    
                    _threadParticipantService.Add(participant1);
                    _threadParticipantService.Add(participant2);
                    
                    // Add to conversation list
                    AddConversation(
                        newThread.ThreadId.ToString(),
                        newThread.Title,
                        "Start a new conversation!",
                        DateTime.Now.ToString("h:mm tt")
                    );
                    
                    // Select the new thread
                    SelectChatThread(newThread.ThreadId.ToString());
                    
                    // Notify the other user via SignalR
                    if (_connection?.State == HubConnectionState.Connected && _currentUser != null)
                    {
                        _connection.InvokeAsync("NotifyNewThread", e.SelectedUser.UserId, _currentUser.Username, newThread.ThreadId);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing user selection: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Helper method to select a chat thread
        private void SelectChatThread(string threadId)
        {
            // Find the conversation item
            var conversation = ConversationList.Conversations.FirstOrDefault(c => c.Id == threadId);
            if (conversation != null)
            {
                // Update UI to show this thread is selected
                ConversationList_ConversationSelected(this, threadId);
                
                // Raise ConversationSelectionChanged event on ConversationList
                ConversationList.SelectConversation(conversation);
            }
            else
            {
                // Refresh conversation list and try again
                LoadChatThreads();
                
                // Try to find conversation again
                conversation = ConversationList.Conversations.FirstOrDefault(c => c.Id == threadId);
                if (conversation != null)
                {
                    // Update UI to show this thread is selected
                    ConversationList_ConversationSelected(this, threadId);
                    
                    // Raise ConversationSelectionChanged event on ConversationList
                    ConversationList.SelectConversation(conversation);
                }
            }
        }
        
        private void ConversationList_AddConversationRequested(object sender, EventArgs e)
        {
            // Reserved for future implementation - group chats
            MessageBox.Show("Creating a group chat is not implemented yet.", "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void ChatView_MenuRequested(object sender, EventArgs e)
        {
            // Reserved for future implementation - chat menu
            MessageBox.Show("Chat options menu is not implemented yet.", "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void ChatView_AttachmentRequested(object sender, EventArgs e)
        {
            // Reserved for future implementation - attachments
            MessageBox.Show("Attachment functionality is not implemented yet.", "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void ConversationList_AdminDashboardRequested(object sender, EventArgs e)
        {
            AdminDashboardPanel.Visibility = System.Windows.Visibility.Visible;
        }
        
        private void CloseAdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            AdminDashboardPanel.Visibility = System.Windows.Visibility.Collapsed;
        }
    }
} 