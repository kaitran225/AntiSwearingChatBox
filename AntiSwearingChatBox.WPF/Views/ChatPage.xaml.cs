using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using AntiSwearingChatBox.WPF.Services;
using AntiSwearingChatBox.WPF.Services.Api;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Media;
using AntiSwearingChatBox.WPF.Models;
using AntiSwearingChatBox.WPF.Components;
using AntiSwearingChatBox.WPF.Models.Api;
using AntiSwearingChatBox.WPF.ViewModels;

namespace AntiSwearingChatBox.WPF.View
{
    public partial class ChatPage : Page
    {
        private HubConnection? _connection;
        private readonly IApiService _apiService;
        private int _currentThreadId;
        private string _currentUsername = string.Empty;
        
        public ChatPage()
        {
            InitializeComponent();
            
            // Get the ApiService from our ServiceProvider
            _apiService = AntiSwearingChatBox.WPF.Services.ServiceProvider.ApiService;
            
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
        
        private void LoadCurrentUser()
        {
            if (_apiService.CurrentUser != null)
            {
                _currentUsername = _apiService.CurrentUser.Username!;
                UpdateUserDisplay(_currentUsername);
            }
            else
            {
                // If no user is logged in, redirect to login page
                MessageBox.Show("No user is logged in. Please login again.", "Session Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.NavigateToLogin();
                }
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentUser();
            await InitializeConnection();
            await LoadChatThreads();
            
            // Initialize with an empty chat view
            ChatView.ClearChat();
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
                    MessageBox.Show("No user is currently logged in.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Clear existing conversations
                ClearConversations();
                
                // Get chat threads from API
                var threads = await _apiService.GetThreadsAsync();
                Console.WriteLine($"Retrieved {threads.Count} threads from API");
                
                // Add each thread to the UI
                foreach (var thread in threads)
                {
                    // Try to get the latest message for the thread
                    var messages = await _apiService.GetMessagesAsync(thread.ThreadId);
                    string lastMessageContent = "No messages yet";
                    string lastMessageTime = thread.CreatedAt.ToString("g");
                    
                    // If there are messages in this thread, display the latest one
                    if (messages != null && messages.Count > 0)
                    {
                        // The messages should be ordered chronologically, 
                        // so the last one is the most recent
                        var latestMessage = messages.LastOrDefault();
                        if (latestMessage != null)
                        {
                            lastMessageContent = latestMessage.ModeratedMessage ?? latestMessage.OriginalMessage;
                            lastMessageTime = latestMessage.CreatedAt.ToString("g");
                        }
                    }
                    
                    // Add conversation to UI with last message info
                    AddConversation(
                        thread.ThreadId.ToString(), 
                        thread.Name ?? $"Chat {thread.ThreadId}",
                        lastMessageContent,
                        lastMessageTime
                    );
                }
                
                // If we have threads, select the first one by default
                if (threads.Count > 0)
                {
                    SelectChatThread(threads[0].ThreadId.ToString());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadChatThreads: {ex}");
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
            ConversationList.Conversations.Add(new ConversationItemViewModel
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
                Console.WriteLine("Initializing SignalR connection...");
                
                // Use the hub URL from ApiConfig
                _connection = new HubConnectionBuilder()
                    .WithUrl(ApiConfig.ChatHubUrl)
                    .WithAutomaticReconnect(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) })
                    .Build();
                
                Console.WriteLine("Registering message handler...");
                
                // Handle incoming messages
                _connection.On<int, string, string, string>("ReceiveMessage", 
                    (threadId, sender, message, timestamp) =>
                {
                    Console.WriteLine($"Received message in thread {threadId} from {sender}: {message}");
                    
                    // Update Conversation List preview
                    Dispatcher.Invoke(() => UpdateConversationLastMessage(threadId.ToString(), message, timestamp));

                    // Only process messages for the current thread in the main view
                    if (threadId == _currentThreadId)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var username = sender ?? "Unknown";
                            var avatar = !string.IsNullOrEmpty(username) ? username[0].ToString().ToUpper() : "?";
                            var isSent = sender == _currentUsername;
                            
                            Console.WriteLine($"Adding message to chat view (isSent: {isSent})");
                            
                            ChatView.Messages.Add(new ChatMessageViewModel
                            {
                                IsSent = isSent,
                                Text = message,
                                Timestamp = timestamp,
                                Avatar = avatar
                            });
                            
                            // Scroll to bottom
                            ChatView.ScrollToBottom(); 
                        });
                    }
                });
                
                try
                {
                    // Connect to the hub
                    await _connection.StartAsync();
                    Console.WriteLine("SignalR connection established successfully");
                    
                    // Join the hub with the current user
                    if (_apiService.CurrentUser != null)
                    {
                        await _connection.InvokeAsync("JoinChat", _currentUsername, _apiService.CurrentUser.UserId);
                        Console.WriteLine($"Joined chat as {_currentUsername}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error connecting to SignalR hub: {ex.Message}");
                    // Continue execution even if SignalR connection fails
                    // The app can still function without real-time updates
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing SignalR connection: {ex}");
                MessageBox.Show($"Error connecting to chat server: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void UpdateConversationLastMessage(string threadId, string message, string time)
        {
            var conversationVM = ConversationList.Conversations.FirstOrDefault(c => c.Id == threadId);
            if (conversationVM != null)
            {
                conversationVM.LastMessage = message;
                conversationVM.LastMessageTime = time;
                // Optionally increase unread count if the chat isn't selected
                if (int.Parse(threadId) != _currentThreadId)
                {
                    conversationVM.UnreadCount++;
                }
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

        private async void ConversationList_ConversationSelected(object sender, string threadId)
        {
            try
            {
                // Parse thread ID
                if (!int.TryParse(threadId, out int parsedThreadId))
                {
                    MessageBox.Show("Invalid thread ID.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                Console.WriteLine($"Selected thread: {threadId}");
                _currentThreadId = parsedThreadId;
                
                // Set up UI immediately to provide user feedback
                foreach (var conv in ConversationList.Conversations)
                {
                    conv.IsSelected = conv.Id == threadId;
                }
                
                // Get the selected conversation from the list
                var conversation = ConversationList.Conversations.FirstOrDefault(c => c.Id == threadId);
                if (conversation != null)
                {
                    // Set the current contact FIRST - this is critical
                    ChatView.CurrentContact = new Components.ContactViewModel
                    {
                        Id = conversation.Id,
                        Name = conversation.Title,
                        Initials = conversation.Avatar,
                    };
                    
                    // Reset unread count for this conversation
                    conversation.UnreadCount = 0;
                }
                
                // Clear existing messages before loading new ones
                ChatView.Messages.Clear();
                
                // Force immediate UI refresh - don't wait for data
                Dispatcher.Invoke(() => {
                    Console.WriteLine("Forcing UI update in ChatView");
                    ChatView.ShowChatView();
                    ChatView.UpdateLayout(); // Force layout update
                });
                
                // Load messages for the selected thread
                Console.WriteLine($"Loading messages for thread {threadId}...");
                var messages = await _apiService.GetMessagesAsync(parsedThreadId);
                Console.WriteLine($"Retrieved {messages?.Count ?? 0} messages from API");
                
                // Add messages to the chat view
                if (messages != null && messages.Count > 0)
                {
                    foreach (var message in messages)
                    {
                        var username = message.User?.Username ?? "Unknown";
                        var avatar = !string.IsNullOrEmpty(username) ? username[0].ToString().ToUpper() : "?";
                        var isSent = message.UserId == _apiService.CurrentUser?.UserId;
                        
                        ChatView.Messages.Add(new ChatMessageViewModel
                        {
                            IsSent = isSent,
                            Text = message.ModeratedMessage ?? message.OriginalMessage,
                            Timestamp = message.CreatedAt.ToString("h:mm tt"),
                            Avatar = avatar
                        });
                    }
                    
                    // After adding all messages, scroll to bottom
                    ChatView.UpdateLayout(); // Force layout update again
                    ChatView.ScrollToBottom();
                }
                else
                {
                    Console.WriteLine("No messages found for this thread");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading thread messages: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error loading messages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void ChatView_MessageSent(object sender, string message)
        {
            try
            {
                // Add the message to the UI immediately
                var avatar = !string.IsNullOrEmpty(_currentUsername) ? _currentUsername[0].ToString().ToUpper() : "?";
                ChatView.Messages.Add(new ChatMessageViewModel
                {
                    IsSent = true,
                    Text = message,
                    Timestamp = DateTime.Now.ToString("h:mm tt"),
                    Avatar = avatar
                });
                
                // Scroll to bottom to show the new message
                ChatView.ScrollToBottom();
                
                // Send to API (this will trigger the server to send to others)
                var result = await _apiService.SendMessageAsync(_currentThreadId, message);
                if (result != null)
                {
                    // Sent successfully
                    Console.WriteLine("Message sent successfully");
                    
                    // Update the timestamp with the server timestamp if needed
                    // (optional as we already show the message with local time)
                }
                else
                {
                    // API returned null - add error indicator to UI
                    Console.WriteLine("Error: API returned null when sending message");
                    
                    // Option 1: Show an error message
                    MessageBox.Show("Failed to send message. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
                    // Option 2: Mark the message as failed in the UI (add a visual indicator)
                    // This would require extending the ChatMessageViewModel with a "Failed" property
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex}");
                MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void ChatView_MenuRequested(object sender, EventArgs e)
        {
            // Handle menu request
        }
        
        private void ChatView_AttachmentRequested(object sender, EventArgs e)
        {
            // Handle attachment request
        }
        
        private void ConversationList_NewChatRequested(object sender, EventArgs e)
        {
            // Handle new chat request
        }
        
        private void ConversationList_AddConversationRequested(object sender, EventArgs e)
        {
            // Handle add conversation request
        }
        
        private void ConversationList_AdminDashboardRequested(object sender, EventArgs e)
        {
            // Handle admin dashboard request
        }
        
        private void CloseAdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle close admin dashboard request
        }

        private void ChatPage_Initialized(object sender, EventArgs e)
        {
            // Subscribe to the events from the ChatView
            ChatView.NewConversationRequested += ConversationList_NewChatRequested!;
        }
    }
} 