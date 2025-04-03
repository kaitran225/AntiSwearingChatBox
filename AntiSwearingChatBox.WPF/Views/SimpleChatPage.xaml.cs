using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.AspNetCore.SignalR.Client;
using AntiSwearingChatBox.WPF.Services.Api;
using AntiSwearingChatBox.WPF.Components;
using AntiSwearingChatBox.WPF.ViewModels;
using System.Linq;
using System.Windows.Input;

namespace AntiSwearingChatBox.WPF.View
{
    /// <summary>
    /// A simplified chat page that combines the conversation list and chat view in a single file
    /// </summary>
    public partial class SimpleChatPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private HubConnection? _connection;
        private readonly IApiService _apiService;
        private int _currentThreadId;
        private string _currentUsername = string.Empty;
        
        private Dictionary<string, DateTime> _recentlySentMessages = new Dictionary<string, DateTime>();
        private object _sendLock = new object();

        // Properties for the conversation list
        public ObservableCollection<ConversationItemViewModel> Conversations { get; private set; }
        public bool IsAdmin { get; set; } = true;  // Default to true for testing
        
        // Properties for the chat view
        public ObservableCollection<ChatMessageViewModel> Messages { get; private set; }
        public string CurrentContactName { get; set; } = string.Empty;
        private bool _hasSelectedConversation = false;
        
        public bool HasSelectedConversation 
        { 
            get => _hasSelectedConversation; 
            set
            {
                _hasSelectedConversation = value;
                OnPropertyChanged(nameof(HasSelectedConversation));
                Console.WriteLine($"HasSelectedConversation set to: {value}");
            }
        }

        public SimpleChatPage()
        {
            InitializeComponent();
            this.DataContext = this;
            
            _apiService = AntiSwearingChatBox.WPF.Services.ServiceProvider.ApiService;
            
            // Initialize collections
            Conversations = new ObservableCollection<ConversationItemViewModel>();
            Messages = new ObservableCollection<ChatMessageViewModel>();
            
            // Make sure the ItemsSource is set
            if (MessagesList != null)
            {
                MessagesList.ItemsSource = Messages;
            }
            
            if (ConversationsItemsControl != null)
            {
                ConversationsItemsControl.ItemsSource = Conversations;
            }
        }

        #region Event Handlers
        
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentUser();
            await InitializeConnection();
            await LoadChatThreads();
            
            // Initialize with empty chat view
            HasSelectedConversation = false;
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

        private void UpdateUserDisplay(string username)
        {
            UserDisplayName.Text = username;
            if (!string.IsNullOrEmpty(username))
            {
                UserInitials.Text = username.Substring(0, 1).ToUpper();
            }
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
                Conversations.Clear();
                
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
                    
                    var threadName = thread.Name ?? $"Chat {thread.ThreadId}";
                    
                    // Extract a good avatar character from the thread name
                    // A thread name might be the other person's username or a custom name
                    var avatarChar = "?";
                    if (!string.IsNullOrEmpty(threadName))
                    {
                        // Get the first character, ensuring it's a letter if possible
                        var firstChar = threadName.Trim().FirstOrDefault(c => char.IsLetter(c));
                        if (firstChar != '\0')
                        {
                            avatarChar = firstChar.ToString().ToUpper();
                        }
                        else if (threadName.Length > 0)
                        {
                            // Fallback to first character if no letter found
                            avatarChar = threadName[0].ToString().ToUpper();
                        }
                    }
                    
                    // Add conversation to UI with last message info
                    Conversations.Add(new ConversationItemViewModel
                    {
                        Id = thread.ThreadId.ToString(),
                        Title = threadName,
                        LastMessage = lastMessageContent,
                        LastMessageTime = lastMessageTime,
                        UnreadCount = 0,
                        IsSelected = false
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadChatThreads: {ex}");
                MessageBox.Show($"Error loading chat threads: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                            
                            Messages.Add(new ChatMessageViewModel
                            {
                                IsSent = false,
                                Text = message,
                                Timestamp = timestamp,
                                Avatar = avatar
                            });
                            
                            // Scroll to bottom to show the new message
                            ScrollToBottom();
                        });
                    }
                    else if (!isCurrentThread)
                    {
                        // If the message is for a different thread, update the unread count
                        Dispatcher.Invoke(() =>
                        {
                            var conversation = Conversations.FirstOrDefault(c => c.Id == threadId.ToString());
                            if (conversation != null)
                            {
                                conversation.UnreadCount++;
                            }
                        });
                    }
                });
                
                // Start the connection
                await _connection.StartAsync();
                Console.WriteLine("SignalR connection started successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing connection: {ex.Message}");
                MessageBox.Show($"Error connecting to chat server: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ConversationItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string threadId)
            {
                Console.WriteLine($"Conversation clicked: {threadId}");
                LoadConversation(threadId);
                e.Handled = true;
            }
            else
            {
                Console.WriteLine($"Conversation clicked but tag is not a string: {(sender as FrameworkElement)?.Tag}");
            }
        }

        private async void LoadConversation(string threadId)
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
                foreach (var conv in Conversations)
                {
                    conv.IsSelected = conv.Id == threadId;
                    
                    // Reset unread count for the selected conversation
                    if (conv.Id == threadId)
                    {
                        conv.UnreadCount = 0;
                    }
                }
                
                // Get the selected conversation from the list
                var conversation = Conversations.FirstOrDefault(c => c.Id == threadId);
                if (conversation != null)
                {
                    // Set the current contact info
                    CurrentContactName = conversation.Title;
                    OnPropertyChanged(nameof(CurrentContactName));
                    
                    // Set HasSelectedConversation to true
                    HasSelectedConversation = true;
                }
                
                // Clear existing messages before loading new ones
                Messages.Clear();
                
                // Force immediate UI refresh - don't wait for data
                Dispatcher.Invoke(() => {
                    Console.WriteLine("Forcing UI update for chat view");
                    UpdateLayout(); // Force layout update
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
                        
                        Messages.Add(new ChatMessageViewModel
                        {
                            IsSent = isSent,
                            Text = message.ModeratedMessage ?? message.OriginalMessage,
                            Timestamp = message.CreatedAt.ToString("h:mm tt"),
                            Avatar = avatar
                        });
                    }
                    
                    // After adding all messages, scroll to bottom
                    UpdateLayout(); // Force layout update again
                    ScrollToBottom();
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

        private void UpdateConversationLastMessage(string threadId, string message, string timestamp)
        {
            var conversation = Conversations.FirstOrDefault(c => c.Id == threadId);
            if (conversation != null)
            {
                conversation.LastMessage = message;
                conversation.LastMessageTime = timestamp;
            }
        }
        
        private void ScrollToBottom()
        {
            try
            {
                if (MessagesScroll != null)
                {
                    MessagesScroll.ScrollToBottom();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error scrolling to bottom: {ex.Message}");
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Implement search functionality
            string searchText = SearchBox.Text.ToLower();
            
            // Simple filter for now - can be enhanced later
            if (string.IsNullOrWhiteSpace(searchText))
            {
                ConversationsItemsControl.ItemsSource = Conversations;
            }
            else
            {
                var filtered = Conversations.Where(c => 
                    c.Title.ToLower().Contains(searchText) || 
                    c.LastMessage.ToLower().Contains(searchText)).ToList();
                    
                ConversationsItemsControl.ItemsSource = filtered;
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle menu button click
            MessageBox.Show("Menu options will be added here.", "Menu", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void NewChat_Click(object sender, RoutedEventArgs e)
        {
            // Handle new chat button click
            MessageBox.Show("New chat functionality will be added here.", "New Chat", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            // Handle admin dashboard button click
            MessageBox.Show("Admin dashboard functionality will be added here.", "Admin Dashboard", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void AttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle attachment button click
            MessageBox.Show("Attachment functionality will be added here.", "Attachment", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void CallButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle audio call button click
            MessageBox.Show($"Audio call with {CurrentContactName} will be implemented in a future version.", 
                "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void VideoCallButton_Click(object sender, RoutedEventArgs e)
        {
            // Handle video call button click
            MessageBox.Show($"Video call with {CurrentContactName} will be implemented in a future version.", 
                "Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle Enter key press to send message
            if (e.Key == Key.Enter)
            {
                // Only handle if not already handled by another handler
                if (e.Handled)
                {
                    Console.WriteLine("Enter key event already handled, skipping");
                    return;
                }
                
                // Shift+Enter creates a new line, just Enter sends the message
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    // Allow Shift+Enter to create a new line
                    Console.WriteLine("Shift+Enter pressed - allowing new line");
                    MessageTextBox.Text += Environment.NewLine;
                    MessageTextBox.CaretIndex = MessageTextBox.Text.Length; // Move caret to end
                    e.Handled = true; // Mark as handled
                }
                else
                {
                    // Just Enter - send the message
                    Console.WriteLine("Enter key pressed without Shift - sending message");
                    e.Handled = true; // Prevent default behavior
                    _ = SendMessage(); // Use discard to acknowledge intentionally not awaiting
                }
            }
        }

        private async Task SendMessage()
        {
            try
            {
                // Get the message text
                string messageText = MessageTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(messageText))
                {
                    return;
                }
                
                // Prevent duplicate sends within a short time window
                lock (_sendLock)
                {
                    // Generate a simple key for the message
                    string messageKey = $"{_currentThreadId}:{messageText}";
                    
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
                
                Console.WriteLine($"Sending message: '{messageText}'");
                
                // Add the message to the UI immediately
                var avatar = !string.IsNullOrEmpty(_currentUsername) ? _currentUsername[0].ToString().ToUpper() : "?";
                Messages.Add(new ChatMessageViewModel
                {
                    IsSent = true,
                    Text = messageText,
                    Timestamp = DateTime.Now.ToString("h:mm tt"),
                    Avatar = avatar
                });
                
                // Clear the text box
                MessageTextBox.Clear();
                
                // Scroll to bottom to show the new message
                ScrollToBottom();
                
                // Send to API (this will trigger the server to send to others)
                Console.WriteLine($"Calling API to send message to thread {_currentThreadId}");
                var result = await _apiService.SendMessageAsync(_currentThreadId, messageText);
                if (result != null)
                {
                    // Sent successfully
                    Console.WriteLine("Message sent successfully through API");
                    
                    // Update the conversation last message
                    UpdateConversationLastMessage(_currentThreadId.ToString(), messageText, DateTime.Now.ToString("h:mm tt"));
                }
                else
                {
                    // API returned null - add error indicator to UI
                    Console.WriteLine("Error: API returned null when sending message");
                    
                    // Show an error message
                    MessageBox.Show("Failed to send message. Please try again.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex}");
                MessageBox.Show($"Error sending message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        #endregion

        protected void OnPropertyChanged(string propertyName)
        {
            Console.WriteLine($"Property changed: {propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 