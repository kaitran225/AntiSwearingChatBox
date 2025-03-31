using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using AntiSwearingChatBox.App.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;

namespace AntiSwearingChatBox.App.Views
{
    public partial class ChatPage : Page
    {
        private HubConnection? _connection;
        private readonly ApiService _apiService;
        private int _currentThreadId;
        private string _currentUsername;
        
        public ChatPage()
        {
            InitializeComponent();
            
            // Get the ApiService from the DI container
            _apiService = ((App)Application.Current).ServiceProvider.GetService<ApiService>();
            
            // Initialize events
            InitializeEvents();
        }
        
        private void InitializeEvents()
        {
            this.Loaded += Page_Loaded;
            
            // Hook up events from UI components
            ChatView.MessageSent += ChatView_MessageSent!;
            ChatView.MenuRequested += ChatView_MenuRequested!;
            ChatView.AttachmentRequested += ChatView_AttachmentRequested!;
            
            ConversationList.ConversationSelected += ConversationList_ConversationSelected!;
            ConversationList.NewChatRequested += ConversationList_NewChatRequested!;
            ConversationList.AddConversationRequested += ConversationList_AddConversationRequested!;
            ConversationList.AdminDashboardRequested += ConversationList_AdminDashboardRequested!;
            
            if (CloseAdminDashboardButton != null)
            {
                CloseAdminDashboardButton.Click += CloseAdminDashboardButton_Click;
            }
        }
        
        private async void LoadCurrentUser()
        {
            if (_apiService.CurrentUser != null)
            {
                _currentUsername = _apiService.CurrentUser.Username;
                UpdateUserDisplay(_currentUsername);
            }
            else
            {
                // If no user is logged in, redirect to login page
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.NavigateToLogin();
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentUser();
            await LoadChatThreads();
            await InitializeConnection();
        }

        private void UpdateUserDisplay(string username)
        {
            // Set the username in the UI
            UserDisplayName.Text = username;
        }

        private async Task LoadChatThreads()
        {
            try
            {
                if (_apiService.CurrentUser == null)
                {
                    return;
                }

                // Clear existing conversations
                ClearConversations();
                
                // Get chat threads from API
                var threads = await _apiService.GetUserThreadsAsync(_apiService.CurrentUser.Id);
                
                // Add each thread to the UI
                foreach (var thread in threads)
                {
                    string lastMessage = thread.LastMessage != null 
                        ? thread.LastMessage.Text 
                        : "No messages yet";
                    
                    string lastMessageTime = thread.LastMessage != null 
                        ? thread.LastMessage.CreatedAt.ToString("h:mm tt") 
                        : "";
                    
                    AddConversation(
                        thread.Id.ToString(), 
                        thread.Title,
                        lastMessage,
                        lastMessageTime
                    );
                }
                
                // If there are threads, select the first one
                if (threads.Count > 0)
                {
                    SelectChatThread(threads[0].Id.ToString());
                }
            }
            catch (Exception ex)
            {
                // Handle error appropriately
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
                // Create SignalR connection for real-time messages
                _connection = new HubConnectionBuilder()
                    .WithUrl("http://localhost:5000/chatHub")
                    .WithAutomaticReconnect()
                    .Build();
                
                // Handle incoming messages
                _connection.On<int, string, string, string>("ReceiveMessage", 
                    (threadId, sender, message, timestamp) =>
                {
                    // Only process messages for the current thread
                    if (threadId == _currentThreadId)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            // Add to chat view
                            ChatView.Messages.Add(new Components.MessageViewModel
                            {
                                IsSent = sender == _currentUsername,
                                Text = message,
                                Timestamp = timestamp,
                                Avatar = sender.Substring(0, 1).ToUpper(),
                                Background = new SolidColorBrush(Colors.LightGray),
                                BorderBrush = new SolidColorBrush(Colors.Gray)
                            });
                        });
                    }
                    
                    // Update conversation list with latest message
                    UpdateConversationLastMessage(threadId.ToString(), message, timestamp);
                });
                
                await _connection.StartAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to chat server: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void UpdateConversationLastMessage(string threadId, string message, string time)
        {
            // Find the conversation in the list and update the last message
            var conversation = ConversationList.Conversations
                .FirstOrDefault(c => c.Id == threadId);
                
            if (conversation != null)
            {
                conversation.LastMessage = message;
                conversation.LastMessageTime = time;
            }
        }
        
        private async void ChatView_MessageSent(object sender, string message)
        {
            if (string.IsNullOrWhiteSpace(message) || _currentThreadId == 0)
                return;
                
            try
            {
                // Send message to API
                var (success, _, sentMessage) = await _apiService.SendMessageAsync(_currentThreadId, message);
                
                if (success && sentMessage != null)
                {
                    // Add message to the chat view
                    ChatView.Messages.Add(new Components.MessageViewModel
                    {
                        IsSent = true,
                        Text = sentMessage.Text,
                        Timestamp = sentMessage.CreatedAt.ToString("h:mm tt"),
                        Avatar = _currentUsername.Substring(0, 1).ToUpper(),
                        Background = new SolidColorBrush(Colors.LightBlue),
                        BorderBrush = new SolidColorBrush(Colors.SkyBlue)
                    });
                    
                    // Update conversation with last message
                    UpdateConversationLastMessage(
                        _currentThreadId.ToString(), 
                        sentMessage.Text, 
                        sentMessage.CreatedAt.ToString("h:mm tt")
                    );
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void ConversationList_ConversationSelected(object sender, string threadId)
        {
            await LoadChatThread(threadId);
        }
        
        private async Task LoadChatThread(string threadId)
        {
            if (string.IsNullOrEmpty(threadId))
                return;
                
            try
            {
                // Parse thread ID
                if (int.TryParse(threadId, out int id))
                {
                    _currentThreadId = id;
                    
                    // Clear existing messages
                    ChatView.Messages.Clear();
                    
                    // Get messages for this thread
                    var messages = await _apiService.GetThreadMessagesAsync(id);
                    
                    // Add messages to the chat view
                    foreach (var message in messages)
                    {
                        bool isSent = message.User?.Username == _currentUsername;
                        
                        ChatView.Messages.Add(new Components.MessageViewModel
                        {
                            IsSent = isSent,
                            Text = message.Text,
                            Timestamp = message.CreatedAt.ToString("h:mm tt"),
                            Avatar = (message.User?.Username?.Substring(0, 1) ?? "?").ToUpper(),
                            Background = isSent 
                                ? new SolidColorBrush(Colors.LightBlue) 
                                : new SolidColorBrush(Colors.LightGray),
                            BorderBrush = isSent
                                ? new SolidColorBrush(Colors.SkyBlue)
                                : new SolidColorBrush(Colors.Gray)
                        });
                    }
                    
                    // Update contact info in chat header
                    var threads = await _apiService.GetUserThreadsAsync(_apiService.CurrentUser.Id);
                    var thread = threads.FirstOrDefault(t => t.Id == id);
                    
                    if (thread != null)
                    {
                        ChatView.CurrentContact = new Components.ContactViewModel
                        {
                            Id = thread.Id.ToString(),
                            Name = thread.Title,
                            Status = "Online",
                            IsOnline = true
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading chat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void ConversationList_NewChatRequested(object sender, EventArgs e)
        {
            try
            {
                // Create a new chat thread
                string newThreadTitle = "New Conversation";
                var (success, _, thread) = await _apiService.CreateThreadAsync(newThreadTitle, false);
                
                if (success && thread != null)
                {
                    // Add the new thread to the UI
                    AddConversation(
                        thread.Id.ToString(),
                        thread.Title,
                        "No messages yet",
                        ""
                    );
                    
                    // Select the new thread
                    SelectChatThread(thread.Id.ToString());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating new chat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void SelectChatThread(string threadId)
        {
            // Find the conversation in the list and select it
            var conversation = ConversationList.Conversations
                .FirstOrDefault(c => c.Id == threadId);
                
            if (conversation != null)
            {
                conversation.IsSelected = true;
                
                // Load the thread
                ConversationList_ConversationSelected(this, threadId);
            }
        }
        
        private void ConversationList_AddConversationRequested(object sender, EventArgs e)
        {
            MessageBox.Show("Add conversation feature is not yet implemented.", "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void ChatView_MenuRequested(object sender, EventArgs e)
        {
            MessageBox.Show("Menu feature is not yet implemented.", "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void ChatView_AttachmentRequested(object sender, EventArgs e)
        {
            MessageBox.Show("Attachment feature is not yet implemented.", "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void ConversationList_AdminDashboardRequested(object sender, EventArgs e)
        {
            MessageBox.Show("Admin dashboard is not yet implemented.", "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void CloseAdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide admin dashboard panel
            if (AdminDashboardPanel != null)
            {
                AdminDashboardPanel.Visibility = Visibility.Collapsed;
            }
        }
    }
} 