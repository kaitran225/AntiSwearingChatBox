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
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        private HubConnection? _connection;
        private readonly IMessageHistoryService _messageHistoryService;
        private readonly IChatThreadService _chatThreadService;
        private readonly IUserService _userService;
        private readonly IThreadParticipantService _threadParticipantService;
        private readonly IAuthService _authService;
        private int _currentUserId;
        private User? _currentUser;
        
        public ChatWindow()
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
            ChatView.MessageSent += ChatView_MessageSent;
            ConversationList.ConversationSelected += ConversationList_ConversationSelected;
            ConversationList.NewChatRequested += ConversationList_NewChatRequested;
            ConversationList.AddConversationRequested += ConversationList_AddConversationRequested;
            ConversationList.AdminDashboardRequested += ConversationList_AdminDashboardRequested;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get current user from login
            if (Tag is User user)
            {
                _currentUser = user;
                _currentUserId = user.UserId;
                // Update UI with user info
                UpdateUserDisplay(user.Username);
            }
            else
            {
                MessageBox.Show("Authentication error. Please log in again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                new LoginWindow().Show();
                Close();
                return;
            }
            
            // Load chat threads
            LoadChatThreads();
            
            // Initialize SignalR connection
            await InitializeConnection();
        }

        private void UpdateUserDisplay(string username)
        {
            // Update UI elements with username - implement this based on your actual UI
            // This is a placeholder method that should be implemented based on your actual UI
            Title = $"Chat - {username} - Anti-Swearing Chat Box";
            
            // If you have specific UI elements for user display, update them here
            // For example:
            // UserNameLabel.Content = username;
            // UserStatusIndicator.Fill = new SolidColorBrush(Colors.Green);
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
            // Implementation depends on your ConversationList control
            ConversationList.Conversations.Clear();
        }
        
        private void AddConversation(string id, string title, string lastMessage, string lastMessageTime)
        {
            // Add a conversation to the UI
            // Implementation depends on your ConversationList control
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
                                    Avatar = isSent ? "ME" : user.Substring(0, 1).ToUpper(),
                                    Background = (Brush)(isSent ? 
                                        Application.Current.Resources["PrimaryBackgroundBrush"] : 
                                        Application.Current.Resources["SecondaryBackgroundBrush"]),
                                    BorderBrush = (Brush)(isSent ? 
                                        Application.Current.Resources["PrimaryGreenBrush"] : 
                                        Application.Current.Resources["BorderBrush"])
                                });
                                
                                // Update last message in conversation list
                                ConversationList.UpdateConversation(
                                    ChatView.CurrentContact.Id,
                                    (isSent ? "You: " : user + ": ") + message,
                                    timestamp.ToString("h:mm tt"));
                            });
                        }
                    }
                });

                _connection.On<List<object>>("AllUsers", (users) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        foreach (dynamic userObj in users)
                        {
                            if (userObj.Id.ToString() != _currentUserId.ToString())
                            {
                                // Check if we already have a conversation with this user
                                bool exists = ConversationList.Conversations
                                    .Any(c => c.Title == userObj.Name.ToString());
                                
                                if (!exists)
                                {
                                    // Create new direct chat if needed
                                    CreateDirectChatForUser(
                                        int.Parse(userObj.Id.ToString()), 
                                        userObj.Name.ToString()
                                    );
                                }
                            }
                        }
                    });
                });

                _connection.On<string, string>("PrivateMessage", (sender, message) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(message, sender, MessageBoxButton.OK, MessageBoxImage.Information);
                    });
                });

                _connection.On<string>("Error", (errorMessage) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    });
                });

                _connection.On<string, int>("UserJoined", (username, userId) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Add system message to chat view
                        if (ChatView.CurrentContact != null && 
                            int.TryParse(ChatView.CurrentContact.Id, out _))
                        {
                            ChatView.Messages.Add(new Components.MessageViewModel
                            {
                                IsSent = false,
                                Text = $"{username} has joined the chat.",
                                Timestamp = DateTime.Now.ToString("h:mm tt"),
                                Avatar = "SYS",
                                Background = (Brush)Application.Current.Resources["SystemMessageBrush"],
                                BorderBrush = (Brush)Application.Current.Resources["BorderBrush"]
                            });
                        }
                    });
                });

                _connection.On<string, int>("JoinConfirmed", (username, userId) =>
                {
                    // Join confirmation received
                    System.Diagnostics.Debug.WriteLine($"Join confirmed for {username} (ID: {userId})");
                });

                _connection.On<List<object>>("UserList", (users) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Received user list with {users.Count} users");
                });

                _connection.On<string>("UserLeft", (username) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Add system message to chat view
                        if (ChatView.CurrentContact != null)
                        {
                            ChatView.Messages.Add(new Components.MessageViewModel
                            {
                                IsSent = false,
                                Text = $"{username} has left the chat.",
                                Timestamp = DateTime.Now.ToString("h:mm tt"),
                                Avatar = "SYS",
                                Background = (Brush)Application.Current.Resources["SystemMessageBrush"],
                                BorderBrush = (Brush)Application.Current.Resources["BorderBrush"]
                            });
                        }
                    });
                });

                // Start the connection
                await _connection.StartAsync();
                
                // Join the chat with user info
                if (_currentUser != null)
                {
                    await _connection.InvokeAsync("JoinChat", _currentUser.Username, _currentUser.UserId);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error connecting to chat server: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private int GetThreadIdForUser(int userId)
        {
            if (ChatView.CurrentContact == null)
                return 0;
                
            if (int.TryParse(ChatView.CurrentContact.Id, out int threadId))
                return threadId;
                
            return 0;
        }
        
        private void CreateDirectChatForUser(int otherUserId, string username)
        {
            try
            {
                // Check if a thread already exists between these two users
                var userThreads = _threadParticipantService.GetByUserId(_currentUserId);
                foreach (var thread in userThreads)
                {
                    var participants = _threadParticipantService.GetByThreadId(thread.ThreadId);
                    if (participants.Count() == 2 && participants.Any(p => p.UserId == otherUserId))
                    {
                        // Thread already exists
                        var existingThread = _chatThreadService.GetById(thread.ThreadId);
                        if (existingThread != null)
                        {
                            ConversationList.AddConversation(
                                existingThread.ThreadId.ToString(),
                                username,
                                "Click to start chatting",
                                existingThread.LastMessageAt.ToString("h:mm tt")
                            );
                            return;
                        }
                    }
                }
                
                // Create a new private thread
                var newThread = new ChatThread
                {
                    Title = username,
                    CreatedAt = DateTime.Now,
                    LastMessageAt = DateTime.Now,
                    IsActive = true,
                    IsPrivate = true,
                    ModerationEnabled = true
                };
                
                var result = _chatThreadService.Add(newThread);
                if (result.success)
                {
                    int threadId = newThread.ThreadId;
                    
                    // Add both users as participants
                    var participant1 = new ThreadParticipant
                    {
                        ThreadId = threadId,
                        UserId = _currentUserId,
                        JoinedAt = DateTime.Now
                    };
                    
                    var participant2 = new ThreadParticipant
                    {
                        ThreadId = threadId,
                        UserId = otherUserId,
                        JoinedAt = DateTime.Now
                    };
                    
                    _threadParticipantService.Add(participant1);
                    _threadParticipantService.Add(participant2);
                    
                    // Add to conversation list
                    ConversationList.AddConversation(
                        threadId.ToString(),
                        username,
                        "Click to start chatting",
                        DateTime.Now.ToString("h:mm tt")
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating direct chat: {ex.Message}");
            }
        }

        private async void ChatView_MessageSent(object sender, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return;
                
            if (_connection?.State == HubConnectionState.Connected && 
                ChatView.CurrentContact != null && 
                int.TryParse(ChatView.CurrentContact.Id, out int threadId))
            {
                try
                {
                    // Send message through SignalR
                    await _connection.InvokeAsync("SendMessage", message, threadId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void ConversationList_ConversationSelected(object sender, string conversationId)
        {
            if (_connection?.State == HubConnectionState.Connected && 
                int.TryParse(conversationId, out int threadId))
            {
                try
                {
                    // Get thread info
                    var thread = _chatThreadService.GetById(threadId);
                    if (thread != null)
                    {
                        // Clear existing messages
                        ChatView.Messages.Clear();
                        
                        // Set current contact
                        ChatView.CurrentContact = new Components.ContactViewModel
                        {
                            Id = threadId.ToString(),
                            Name = thread.Title,
                            Initials = thread.Title.Substring(0, 1).ToUpper(),
                            Status = "Online"
                        };
                        
                        // Join the thread to receive messages
                        await _connection.InvokeAsync("JoinThread", threadId);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading thread: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_connection != null)
            {
                try
                {
                    await _connection.StopAsync();
                    await _connection.DisposeAsync();
                }
                catch
                {
                    // Ignore errors on closing
                }
            }
        }
        
        private void CloseAdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide admin dashboard
            AdminDashboardPanel.Visibility = Visibility.Collapsed;
            // Show main chat interface
            ChatView.Visibility = Visibility.Visible;
        }

        private void ConversationList_NewChatRequested(object? sender, EventArgs e)
        {
            // Show UI for creating a new chat
            // For example, show a dialog with list of users to chat with
            try
            {
                // Get all users except current user
                var users = _userService.GetAll().Where(u => u.UserId != _currentUserId).ToList();
                
                if (users.Any())
                {
                    // In a real app, show a nice dialog with user selection
                    // For now, just show a message with the first user
                    var selectedUser = users.First();
                    
                    // Create a direct chat with the selected user
                    CreateDirectChatForUser(selectedUser.UserId, selectedUser.Username);
                    
                    MessageBox.Show($"Started a new chat with {selectedUser.Username}", "New Chat", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("No other users available to chat with.", "New Chat", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating new chat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ConversationList_AddConversationRequested(object? sender, EventArgs e)
        {
            // Show UI for adding a new conversation (e.g., creating a group chat)
            MessageBox.Show("Add conversation feature not yet implemented.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void ConversationList_AdminDashboardRequested(object? sender, EventArgs e)
        {
            // Show admin dashboard
            if (AdminDashboardPanel != null)
            {
                ChatView.Visibility = Visibility.Collapsed;
                AdminDashboardPanel.Visibility = Visibility.Visible;
            }
        }

        private void ChatView_PhoneCallRequested(object sender, EventArgs e)
        {
            // Since this is a text-only chat application, display a message that this feature is not available
            MessageBox.Show("Phone call feature is not available in this text-only chat application.", 
                "Feature Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ChatView_VideoCallRequested(object sender, EventArgs e)
        {
            // Since this is a text-only chat application, display a message that this feature is not available
            MessageBox.Show("Video call feature is not available in this text-only chat application.", 
                "Feature Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ChatView_MenuRequested(object sender, EventArgs e)
        {
            // Since this is a text-only chat application, display a message that this feature is not available
            MessageBox.Show("Additional menu options are not available in this text-only chat application.", 
                "Feature Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ChatView_AttachmentRequested(object sender, EventArgs e)
        {
            // Since this is a text-only chat application, display a message that this feature is not available
            MessageBox.Show("File attachment feature is not available in this text-only chat application.", 
                "Feature Not Available", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
} 