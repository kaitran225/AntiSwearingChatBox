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
                    MessageBox.Show("No user is currently logged in.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Console.WriteLine($"Loading chat threads for user {_apiService.CurrentUser.UserId}...");

                // Clear existing conversations
                ClearConversations();
                Console.WriteLine("Cleared existing conversations");
                
                // Get chat threads from API
                var threads = await _apiService.GetThreadsAsync();
                Console.WriteLine($"Retrieved {threads.Count} threads from API");
                
                // Add each thread to the UI
                foreach (var thread in threads)
                {
                    Console.WriteLine($"Adding thread: ID={thread.ThreadId}, Title={thread.Name ?? "Untitled"}");
                    
                    string lastMessage = "No messages yet";
                    string lastMessageTime = thread.CreatedAt.ToString("g");
                    
                    AddConversation(
                        thread.ThreadId.ToString(), 
                        thread.Name ?? $"Chat {thread.ThreadId}",
                        lastMessage,
                        lastMessageTime
                    );
                }
                
                // If there are threads, select the first one
                if (threads.Count > 0)
                {
                    Console.WriteLine($"Selecting first thread: {threads[0].ThreadId}");
                    SelectChatThread(threads[0].ThreadId.ToString());
                }
                else
                {
                    Console.WriteLine("No threads found for user");
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
                    
                    // Only process messages for the current thread
                    if (threadId == _currentThreadId)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var username = sender ?? "Unknown";
                            var avatar = !string.IsNullOrEmpty(username) ? username[0].ToString().ToUpper() : "?";
                            var isSent = sender == _currentUsername;
                            
                            Console.WriteLine($"Adding message to chat view (isSent: {isSent})");
                            
                            // Add to chat view
                            ChatView.Messages.Add(new ChatMessageViewModel
                            {
                                IsSent = isSent,
                                Text = message,
                                Timestamp = timestamp,
                                Avatar = avatar,
                                Background = new SolidColorBrush(isSent ? Color.FromRgb(220, 248, 198) : Color.FromRgb(255, 255, 255)),
                                BorderBrush = new SolidColorBrush(isSent ? Color.FromRgb(206, 233, 185) : Color.FromRgb(229, 229, 229))
                            });
                        });
                    }
                });
                
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
                Console.WriteLine($"Error initializing SignalR connection: {ex}");
                MessageBox.Show($"Error connecting to chat server: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
                _currentThreadId = int.Parse(threadId);
                
                // Clear existing messages
                ChatView.Messages.Clear();
                
                // Load messages for the selected thread
                var messages = await _apiService.GetMessagesAsync(int.Parse(threadId));
                
                // Add messages to the chat view
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
                        Avatar = avatar,
                        Background = new SolidColorBrush(isSent ? Color.FromRgb(220, 248, 198) : Color.FromRgb(255, 255, 255)),
                        BorderBrush = new SolidColorBrush(isSent ? Color.FromRgb(206, 233, 185) : Color.FromRgb(229, 229, 229))
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading thread messages: {ex}");
                MessageBox.Show($"Error loading messages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void ChatView_MessageSent(object sender, string message)
        {
            try
            {
                var result = await _apiService.SendMessageAsync(_currentThreadId, message);
                if (result != null)
                {
                    // Sent successfully
                    Console.WriteLine("Message sent successfully");
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
    }
} 