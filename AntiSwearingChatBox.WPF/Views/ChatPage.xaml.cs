using System.Windows;
using System.Windows.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using AntiSwearingChatBox.WPF.Services.Api;
using AntiSwearingChatBox.WPF.Components;
using AntiSwearingChatBox.WPF.ViewModels;

namespace AntiSwearingChatBox.WPF.View
{
    public partial class ChatPage : Page
    {
        private HubConnection? _connection;
        private readonly IApiService _apiService;
        private int _currentThreadId;
        private string _currentUsername = string.Empty;
        
        private Dictionary<string, DateTime> _recentlySentMessages = new Dictionary<string, DateTime>();
        private object _sendLock = new object();
        
        public ChatPage()
        {
            InitializeComponent();
            
            _apiService = AntiSwearingChatBox.WPF.Services.ServiceProvider.ApiService;
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
            
            // Initialize with an empty chat view and ensure the empty state is shown
            ChatView.ClearChat();
            
            // Make sure the empty state is visible by explicitly notifying property change
            ChatView.OnPropertyChanged(nameof(ChatView.HasSelectedConversation));
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
                
                // Don't automatically select the first conversation
                // Let the user make a selection to show the chat view
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
                    
                    // Track message details for duplication check
                    var isCurrentThread = threadId == _currentThreadId;
                    var isFromSelf = string.Equals(sender, _currentUsername, StringComparison.OrdinalIgnoreCase);
                    
                    Console.WriteLine($"Message check - isCurrentThread: {isCurrentThread}, isFromSelf: {isFromSelf}, currentUsername: '{_currentUsername}', sender: '{sender}'");
                    
                    // Always update the conversation list preview, even for our own messages
                    Dispatcher.Invoke(() => UpdateConversationLastMessage(threadId.ToString(), message, timestamp));

                    // Only process messages for the current thread AND from other users
                    // This prevents duplicate messages when we send a message
                    if (isCurrentThread && !isFromSelf)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            var username = sender ?? "Unknown";
                            var avatar = !string.IsNullOrEmpty(username) ? username[0].ToString().ToUpper() : "?";
                            
                            Console.WriteLine($"Adding message from {sender} to chat view");
                            
                            ChatView.Messages.Add(new ChatMessageViewModel
                            {
                                IsSent = false, // Never our own messages here (those are added in ChatView_MessageSent)
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
                    
                    // Debug - check visibility of message controls
                    var messagesScroll = ChatView.FindName("MessagesScroll") as ScrollViewer;
                    var messagesList = ChatView.FindName("MessagesList") as ItemsControl;
                    Console.WriteLine($"ChatView.MessagesScroll visibility: {messagesScroll?.Visibility}");
                    Console.WriteLine($"ChatView.MessagesList visibility: {messagesList?.Visibility}");
                    Console.WriteLine($"ChatView HasSelectedConversation: {ChatView.HasSelectedConversation}");
                    Console.WriteLine($"Messages count: {ChatView.Messages?.Count}");
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
                // Check for duplicate message sends within a short time window
                lock (_sendLock)
                {
                    // Generate a simple key for the message
                    string messageKey = $"{_currentThreadId}:{message}";
                    
                    // Check if this exact message was sent in the last second
                    if (_recentlySentMessages.TryGetValue(messageKey, out DateTime lastSent))
                    {
                        TimeSpan elapsed = DateTime.Now - lastSent;
                        if (elapsed.TotalSeconds < 2)
                        {
                            Console.WriteLine($"DUPLICATE DETECTED: Same message sent again within {elapsed.TotalSeconds:F1} seconds, ignoring");
                            return;
                        }
                    }
                    
                    // Record this message as sent
                    _recentlySentMessages[messageKey] = DateTime.Now;
                    
                    // Clean up old messages (older than 5 seconds)
                    var keysToRemove = _recentlySentMessages
                        .Where(kvp => (DateTime.Now - kvp.Value).TotalSeconds > 5)
                        .Select(kvp => kvp.Key)
                        .ToList();
                    
                    foreach (var key in keysToRemove)
                    {
                        _recentlySentMessages.Remove(key);
                    }
                }
                
                Console.WriteLine($"ChatView_MessageSent handling message: '{message}'");
                
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
                Console.WriteLine($"Calling API to send message to thread {_currentThreadId}");
                var result = await _apiService.SendMessageAsync(_currentThreadId, message);
                if (result != null)
                {
                    // Sent successfully
                    Console.WriteLine("Message sent successfully through API");
                }
                else
                {
                    // API returned null - add error indicator to UI
                    Console.WriteLine("Error: API returned null when sending message");
                    
                    // Option 1: Show an error message
                    MessageBox.Show("Failed to send message. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            // This event was redundantly adding event handlers
            // Keep it empty to avoid double registration
            Console.WriteLine("ChatPage_Initialized called - keeping empty to avoid duplicate event registrations");
            
            // Do not register events here, it's already done in InitializeEvents()
            // ChatView.NewConversationRequested += ConversationList_NewChatRequested!;
        }
    }
} 