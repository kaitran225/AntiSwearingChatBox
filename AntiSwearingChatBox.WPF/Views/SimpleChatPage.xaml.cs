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
using AntiSwearingChatBox.WPF.Models.Api;
using System.Linq;
using System.Windows.Input;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;

namespace AntiSwearingChatBox.WPF.View
{
    /// <summary>
    /// A simplified chat page that combines the conversation list and chat view in a single file
    /// </summary>
    public partial class SimpleChatPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly IApiService _apiService;
        private int _currentThreadId;
        private string _currentUsername = string.Empty;
        private System.Timers.Timer? _connectionCheckTimer;
        private const int CONNECTION_CHECK_INTERVAL_MS = 5000; // Check every 5 seconds
        
        // Swearing score tracking
        private int _swearingScore = 0;
        public int SwearingScore 
        { 
            get => _swearingScore; 
            set 
            { 
                if (_swearingScore != value)
            { 
                _swearingScore = value;
                OnPropertyChanged(nameof(SwearingScore));
                    
                    // Force UI refresh to ensure score displays correctly
                    RefreshUIElements();
                
                // Check if the score exceeds the limit
                if (_swearingScore > 5)
                {
                    _ = CloseThreadDueToExcessiveSwearing();
                }
                else if (_swearingScore > 0)
                {
                    // Show appropriate warning based on score
                    ShowSwearingWarning(_swearingScore);
                    }
                }
            }
        }
        
        // Flag to indicate if thread is closed due to excessive swearing
        private bool _isThreadClosed = false;
        public bool IsThreadClosed 
        { 
            get => _isThreadClosed; 
            set 
            { 
                _isThreadClosed = value; 
                OnPropertyChanged(nameof(IsThreadClosed));
                OnPropertyChanged(nameof(CanSendMessages));
            } 
        }
        
        // Property to check if messages can be sent
        public bool CanSendMessages => HasSelectedConversation && !IsThreadClosed;
        
        private Dictionary<string, DateTime> _recentlySentMessages = new Dictionary<string, DateTime>();
        private object _sendLock = new object();

        // Properties for the conversation list
        public ObservableCollection<ConversationItemViewModel> Conversations { get; private set; }
        public bool IsAdmin { get; set; } = true;  // Default to true for testing
        
        // Properties for the chat view
        private ObservableCollection<ChatMessageViewModel> _chatMessages = new ObservableCollection<ChatMessageViewModel>();
        public ObservableCollection<ChatMessageViewModel> Messages
        {
            get => _chatMessages;
            private set
            {
                _chatMessages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }
        public string CurrentContactName { get; set; } = string.Empty;
        private bool _hasSelectedConversation = false;
        
        public bool HasSelectedConversation 
        { 
            get => _hasSelectedConversation; 
            set
            {
                if (_hasSelectedConversation != value)
                {
                    _hasSelectedConversation = value;
                    OnPropertyChanged(nameof(HasSelectedConversation));
                    OnPropertyChanged(nameof(CanSendMessages));
                    Console.WriteLine($"HasSelectedConversation set to: {value}, CanSendMessages: {CanSendMessages}");
                }
            }
        }

        private bool _isConnected = true;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                _isConnected = value;
                OnPropertyChanged(nameof(IsConnected));
                UpdateConnectionStatus();
            }
        }

        private System.Timers.Timer? _scorePollingTimer;
        private const int SCORE_POLLING_INTERVAL_MS = 3000; // Check every 3 seconds

        // Method to force UI refresh for swearing score and other indicators
        private void RefreshUIElements()
        {
            Dispatcher.Invoke(() => {
                OnPropertyChanged(nameof(SwearingScore));
                OnPropertyChanged(nameof(IsThreadClosed));
                OnPropertyChanged(nameof(CanSendMessages));
                UpdateLayout();
            });
        }

        public SimpleChatPage()
        {
            InitializeComponent();
            this.DataContext = this;
            
            _apiService = AntiSwearingChatBox.WPF.Services.ServiceProvider.ApiService;
            
            // Initialize collections
            Conversations = new ObservableCollection<ConversationItemViewModel>();
            Messages = new ObservableCollection<ChatMessageViewModel>();
            
            // Initialize properties
            HasSelectedConversation = false;
            IsThreadClosed = false;
            SwearingScore = 0;
            
            // Make sure the ItemsSource is set
            if (MessagesList != null)
            {
                MessagesList.ItemsSource = Messages;
            }
            
            if (ConversationsItemsControl != null)
            {
                ConversationsItemsControl.ItemsSource = Conversations;
            }
            
            // Register page unload event
            this.Unloaded += Page_Unloaded;
        }

        
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCurrentUser();
            await InitializeConnection();
            await LoadChatThreads();
            
            // Ensure UI state is correctly initialized
            HasSelectedConversation = false;
            UpdateLayout();
            
            // Log UI state for debugging
            Console.WriteLine($"Page loaded - HasSelectedConversation: {HasSelectedConversation}, " +
                              $"IsThreadClosed: {IsThreadClosed}, CanSendMessages: {CanSendMessages}");
                              
            // Start connection checking timer
            StartConnectionChecker();
            StartScorePolling();
        }
        
        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Console.WriteLine("Page unloading - cleaning up resources");
                
                // Stop the connection checker timer
                StopConnectionChecker();
                StopScorePolling();
                
                // Unsubscribe from event handlers
                if (_apiService != null)
                {
                    // If we're in a thread, leave the SignalR group
                    if (_currentThreadId > 0)
                    {
                        try
                        {
                            Console.WriteLine($"Leaving SignalR group for thread {_currentThreadId}...");
                            await _apiService.LeaveThreadChatGroupAsync(_currentThreadId);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error leaving thread SignalR group: {ex.Message}");
                        }
                    }
                    
                    _apiService.OnMessageReceived -= HandleMessageReceived;
                    _apiService.OnThreadInfoUpdated -= HandleThreadInfoUpdated;
                    _apiService.OnThreadClosed -= HandleThreadClosed;
                    
                    // Disconnect from SignalR
                    await _apiService.DisconnectFromHubAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during page unload: {ex.Message}");
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

        private void UpdateUserDisplay(string username)
        {
            if (UserDisplayName != null)
        {
            UserDisplayName.Text = username;
            }
            
            if (UserInitials != null && !string.IsNullOrEmpty(username))
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
                    var newConversation = new ConversationItemViewModel
                    {
                        Id = thread.ThreadId.ToString(),
                        Title = threadName,
                        LastMessage = lastMessageContent,
                        LastMessageTime = lastMessageTime,
                        AvatarText = avatarChar,
                        UnreadCount = 0,
                        IsSelected = false
                    };
                    newConversation.SwearingScore = thread.SwearingScore;
                    newConversation.IsClosed = thread.IsClosed;
                    Conversations.Add(newConversation);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in LoadChatThreads: {ex}");
                MessageBox.Show($"Error loading chat threads: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void StartConnectionChecker()
        {
            // Dispose any existing timer
            StopConnectionChecker();
            
            // Create a new timer that checks connection state
            _connectionCheckTimer = new System.Timers.Timer(CONNECTION_CHECK_INTERVAL_MS);
            _connectionCheckTimer.Elapsed += async (sender, e) => await CheckSignalRConnection();
            _connectionCheckTimer.Start();
            
            Console.WriteLine($"SignalR connection checker started with interval of {CONNECTION_CHECK_INTERVAL_MS}ms");
        }
        
        private void StopConnectionChecker()
        {
            if (_connectionCheckTimer != null)
            {
                _connectionCheckTimer.Stop();
                _connectionCheckTimer.Dispose();
                _connectionCheckTimer = null;
                Console.WriteLine("SignalR connection checker stopped");
            }
        }
        
        private async Task CheckSignalRConnection()
        {
            try
            {
                // Check if the SignalR connection is still active
                bool isConnected = await _apiService.IsHubConnectedAsync();
                Console.WriteLine($"[CONNECTION CHECK] SignalR connection state check - Connected: {isConnected}");
                
                // Only update UI if connection state changed
                if (IsConnected != isConnected)
                {
                    Console.WriteLine($"[CONNECTION CHECK] SignalR connection state changed: {IsConnected} -> {isConnected}");
                    
                    // Update the connection status
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsConnected = isConnected;
                        UpdateConnectionStatus();
                    });
                    
                    // Try to reconnect if disconnected
                    if (!isConnected)
                    {
                        Console.WriteLine("[CONNECTION CHECK] Connection lost. Attempting to reconnect...");
                        try
                        {
                            // Try to reconnect to the hub
                            await _apiService.ConnectToHubAsync();
                            
                            // Delay a bit to allow connection to establish
                            await Task.Delay(2000); 
                            
                            // Check if reconnection was successful
                            bool reconnected = await _apiService.IsHubConnectedAsync();
                            Console.WriteLine($"[CONNECTION CHECK] Reconnect attempt result: {(reconnected ? "SUCCESS" : "FAILED")}");
                            
                            // Update UI with new status
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                IsConnected = reconnected;
                                UpdateConnectionStatus();
                            });
                            
                            // If reconnected, re-join current thread
                            if (reconnected && _currentThreadId > 0)
                            {
                                Console.WriteLine($"[CONNECTION CHECK] Successfully reconnected. Rejoining thread {_currentThreadId}");
                                await _apiService.JoinThreadChatGroupAsync(_currentThreadId);
                            }
                        }
                        catch (Exception reconnectEx)
                        {
                            Console.WriteLine($"[CONNECTION CHECK] Error during reconnection: {reconnectEx.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONNECTION CHECK] Error checking SignalR connection: {ex.Message}");
            }
        }
        
        private void UpdateConnectionStatus()
        {
            // Update connection indicator based on IsConnected
            if (ConnectionIndicator != null)
            {
                ConnectionIndicator.Fill = IsConnected ? 
                    new SolidColorBrush(Colors.Green) : 
                    new SolidColorBrush(Colors.Red);
                
                ConnectionStatusText.Text = IsConnected ? 
                    "Connected" : 
                    "Disconnected";
            }
        }
        
        private async Task InitializeConnection()
        {
            try
            {
                Console.WriteLine("Initializing SignalR connection...");
                
                // Use the API service's built-in SignalR connection
                bool connectionResult = await _apiService.ConnectToHubAsync();
                
                // Subscribe to message events from the API service
                _apiService.OnMessageReceived += HandleMessageReceived;
                _apiService.OnThreadInfoUpdated += HandleThreadInfoUpdated;
                _apiService.OnThreadClosed += HandleThreadClosed;
                
                // Verify connection is working
                bool isConnected = await _apiService.IsHubConnectedAsync();
                Console.WriteLine($"[CONNECT] SignalR connection initialized - IsConnected: {isConnected}, Connection Result: {connectionResult}");
                
                IsConnected = isConnected;
                UpdateConnectionStatus();
                
                // Don't show warning popup anymore, just log to console
                if (!isConnected)
                {
                    Console.WriteLine("[WARNING] Real-time chat connection could not be established. Messages may be delayed.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Error initializing connection: {ex.Message}");
                IsConnected = false;
                UpdateConnectionStatus();
            }
        }
        
        private void HandleMessageReceived(ChatMessage message)
        {
            if (message == null)
            {
                Console.WriteLine("WARNING: Received null message in HandleMessageReceived");
                return;
            }
            
            try
            {
                // Log receipt of message
                Console.WriteLine($"Received message from {message.User?.Username ?? "Unknown"}: {message.ModeratedMessage ?? message.OriginalMessage}");
                
                // Ensure we update the UI on the UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    try
                    {
                        // Always update conversation list when receiving new messages
                        // Find the conversation in the list that matches the message's thread
                        var conversation = Conversations.FirstOrDefault(c => c.Id == message.ThreadId.ToString());
                        if (conversation != null)
                        {
                            // Update last message and time in conversation list
                            conversation.LastMessage = message.ModeratedMessage ?? message.OriginalMessage;
                            conversation.LastMessageTime = message.CreatedAt.ToString("g");
                            
                            // If this message was modified (contained profanity), update the thread's swearing score
                            if (message.WasModified && message.ThreadId == _currentThreadId)
                            {
                                // Get the swearing score from the message or use the current one
                                int newScore = message.ThreadSwearingScore ?? _swearingScore;
                                if (newScore > _swearingScore)
                                {
                                    // Update the scoring in the UI immediately
                                    SwearingScore = newScore;
                                    conversation.SwearingScore = newScore;
                                    
                                    // Refresh UI components to ensure swearing score displays properly
                                    RefreshUIElements();
                                    
                                    // Update UI and show a warning if needed
                                    if (newScore > 0)
                                    {
                                        ShowSwearingWarning(newScore);
                                    }
                                }
                            }
                            
                            // If the thread is different from the current one, increment unread count
                            if (message.ThreadId != _currentThreadId)
                            {
                                conversation.UnreadCount++;
                            }
                            
                            // Move updated conversation to the top of the list if it's not already there
                            if (Conversations.Count > 1 && Conversations.IndexOf(conversation) > 0)
                            {
                                Conversations.Remove(conversation);
                                Conversations.Insert(0, conversation);
                            }
                        }
                        
                        // Check if this message belongs to the current thread
                        if (_currentThreadId > 0 && message.ThreadId != _currentThreadId)
                        {
                            Console.WriteLine($"Message is for thread {message.ThreadId}, but we're viewing {_currentThreadId} - not adding to UI");
                            return;
                        }

                        // Check if this message is already in the Messages collection to avoid duplicates
                        bool isDuplicate = Messages.Any(m => 
                            m.Text == (message.ModeratedMessage ?? message.OriginalMessage) &&
                            m.Timestamp == message.CreatedAt.ToString("h:mm tt") &&
                            m.Avatar == (!string.IsNullOrEmpty(message.User?.Username) ? message.User.Username[0].ToString().ToUpper() : "?"));
                        
                        if (isDuplicate)
                        {
                            Console.WriteLine($"Duplicate message detected, not adding to UI again");
                            return;
                        }

                        // Add message to chat
                        Messages.Add(new ChatMessageViewModel
                        {
                            IsSent = message.UserId == _apiService.CurrentUser?.UserId,
                            Text = message.ModeratedMessage ?? message.OriginalMessage,
                            OriginalText = message.OriginalMessage,
                            Timestamp = message.CreatedAt.ToString("h:mm tt"),
                            Avatar = !string.IsNullOrEmpty(message.User?.Username) ? message.User.Username[0].ToString().ToUpper() : "?",
                            ContainsProfanity = message.WasModified,
                            IsUncensored = false // Initially show censored version
                        });
                        
                        // Only auto-scroll if the user is already at the bottom
                        if (IsScrolledToBottom())
                        {
                            ScrollToBottom();
                        }
                        
                        // Play a sound notification for new messages (only for messages not from current user)
                        if (message.UserId != _apiService.CurrentUser?.UserId)
                        {
                            try
                            {
                                System.Media.SystemSounds.Asterisk.Play();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error playing notification sound: {ex.Message}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error updating UI with received message: {ex.Message}");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HandleMessageReceived: {ex.Message}");
            }
        }

        private void HandleThreadInfoUpdated(int threadId, int swearingScore, bool isClosed)
        {
            try
            {
                Console.WriteLine($"[THREAD INFO] Received thread info update - Thread: {threadId}, SwearingScore: {swearingScore}, IsClosed: {isClosed}");
                
                // Update UI if this is the current thread
                if (threadId == _currentThreadId)
                {
                    Console.WriteLine($"[THREAD INFO] Updating current thread info - SwearingScore: {swearingScore}, IsClosed: {isClosed}");
                    
                    // Update the UI on the dispatcher thread
                    Dispatcher.Invoke(() => {
                        SwearingScore = swearingScore;
                        IsThreadClosed = isClosed;
                        
                        // Force UI refresh
                        RefreshUIElements();
                    });
                }
                
                // Update conversation list item if it exists
                var conversation = Conversations.FirstOrDefault(c => c.Id == threadId.ToString());
                if (conversation != null)
                {
                    Dispatcher.Invoke(() => {
                        conversation.SwearingScore = swearingScore;
                        conversation.IsClosed = isClosed;
                    });
                }
                    }
                    catch (Exception ex)
                    {
                Console.WriteLine($"[THREAD INFO ERROR] Error handling thread info update: {ex.Message}");
            }
        }
        
        private void HandleThreadClosed(int threadId, string reason)
        {
            try
            {
                Console.WriteLine($"[THREAD CLOSED] Thread {threadId} was closed: {reason}");
                
                // Update UI if this is the current thread
                if (threadId == _currentThreadId)
                {
                    Console.WriteLine($"[THREAD CLOSED] Current thread was closed: {reason}");
                    
                    // Update the UI on the dispatcher thread
                    Dispatcher.Invoke(() => {
                        IsThreadClosed = true;
                        
                        // Add a system message
                        Messages.Add(new ChatMessageViewModel
                        {
                            IsSent = false,
                            Text = $"⚠️ {reason}",
                            Timestamp = DateTime.Now.ToString("h:mm tt"),
                            Avatar = "🔒"
                        });
                        
                        // Force UI refresh
                        RefreshUIElements();
                        
                        // Show a message box if the user is actively using this thread
                        MessageBox.Show(reason, "Conversation Closed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    });
                }
                
                // Update conversation list item if it exists
                var conversation = Conversations.FirstOrDefault(c => c.Id == threadId.ToString());
                if (conversation != null)
                {
                    Dispatcher.Invoke(() => {
                        conversation.IsClosed = true;
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[THREAD CLOSED ERROR] Error handling thread closed event: {ex.Message}");
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
                
                // Prevent sending if thread is closed
                if (IsThreadClosed)
                {
                    MessageBox.Show("This conversation has been closed due to excessive swearing.", 
                        "Conversation Closed", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                
                Console.WriteLine($"SENDING: '{messageText}' to thread {_currentThreadId}");
                
                // Add the message to the UI immediately for responsive feedback
                var avatar = !string.IsNullOrEmpty(_currentUsername) ? _currentUsername[0].ToString().ToUpper() : "?";
                var currentTimestamp = DateTime.Now.ToString("h:mm tt");
                
                var sentMessage = new ChatMessageViewModel
                {
                    IsSent = true,
                    Text = messageText,
                    OriginalText = messageText,
                    Timestamp = currentTimestamp,
                    Avatar = avatar,
                    ContainsProfanity = false // We don't know yet, will be set by server response
                };
                
                Messages.Add(sentMessage);
                
                // Clear the text box
                MessageTextBox.Clear();
                
                // Scroll to bottom to show the new message
                ScrollToBottom();
                
                // Send to API (this will trigger the server to notify all clients via SignalR)
                Console.WriteLine($"Calling API to send message to thread {_currentThreadId}");
                var result = await _apiService.SendMessageAsync(_currentThreadId, messageText);
                
                if (result != null && !string.IsNullOrEmpty(result.ModeratedMessage))
                {
                    // Sent successfully
                    Console.WriteLine("Message sent successfully through API");
                    
                    // Update the local message with the server's version (which might be moderated)
                    sentMessage.Text = result.ModeratedMessage;
                    sentMessage.ContainsProfanity = result.WasModified;
                    
                    // Handle swearing score updates
                    if (result.WasModified)
                    {
                        SwearingScore++;
                        Console.WriteLine($"Swearing score increased to {SwearingScore}");

                        // Update the swearing score on the server
                        _apiService.UpdateThreadSwearingScoreAsync(_currentThreadId, SwearingScore);
                        
                        // Force UI refresh for swearing score and other indicators
                        RefreshUIElements();
                        
                        // Poll for the latest swearing score immediately
                        Task.Run(async () => {
                            // Short delay to let server finish processing
                            await Task.Delay(500);
                            
                            try {
                                int score = await _apiService.GetThreadSwearingScoreAsync(_currentThreadId);
                                bool isClosed = await _apiService.IsThreadClosedAsync(_currentThreadId);
                                
                                Console.WriteLine($"[SEND] Latest server data after send - SwearingScore: {score}, IsClosed: {isClosed}");
                                
                                // Update on dispatcher thread if different
                                if (score != SwearingScore || isClosed != IsThreadClosed) {
                                    await Dispatcher.InvokeAsync(() => {
                                        SwearingScore = score;
                                        IsThreadClosed = isClosed;
                                        RefreshUIElements();
                                    });
                                }
                            }
                            catch (Exception pollEx) {
                                Console.WriteLine($"[SEND] Error polling for latest score: {pollEx.Message}");
                            }
                        });

                        // Show warning based on swearing score
                        if (SwearingScore > 0)
                        {
                            ShowSwearingWarning(SwearingScore);
                        }

                        // Check if the thread needs to be closed due to excessive swearing
                        if (SwearingScore > 5)
                        {
                            await CloseThreadDueToExcessiveSwearing();
                        }
                    }
                    
                    // Update the conversation last message
                    UpdateConversationLastMessage(_currentThreadId.ToString(), result.ModeratedMessage, currentTimestamp);
                }
                else
                {
                    // API returned null or empty - add error indicator to UI
                    Console.WriteLine("Error: API returned null or empty when sending message");
                    
                    // Mark the message with an error indicator
                    sentMessage.Text += " ⚠️";
                    sentMessage.SendFailed = true;
                    
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

        private void UpdateConversationLastMessage(string threadId, string message, string timestamp)
        {
            var conversation = Conversations.FirstOrDefault(c => c.Id == threadId);
            if (conversation != null)
            {
                conversation.LastMessage = message;
                conversation.LastMessageTime = timestamp;
                
                // Move this conversation to the top of the list (most recent)
                if (Conversations.Count > 1 && Conversations.IndexOf(conversation) > 0)
                {
                    Dispatcher.Invoke(() => {
                        Conversations.Remove(conversation);
                        Conversations.Insert(0, conversation);
                    });
                }
            }
        }
        
        private bool IsScrolledToBottom()
        {
            if (MessagesScroll == null)
                return true;
                
            // Check if scroll viewer is at the bottom
            return MessagesScroll.VerticalOffset >= MessagesScroll.ScrollableHeight - 20;
        }
        
        private void ScrollToBottom()
        {
            if (MessagesScroll == null)
                return;
                
            MessagesScroll.ScrollToEnd();
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
            try
            {
                // Call our existing dialog implementation instead of using NewChatDialog class
                NewChatDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating new chat: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void AITest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Navigate to the AI Test page
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.NavigateToAITest();
                }
                else
                {
                    MessageBox.Show("Cannot navigate to AI Test page.", "Navigation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error navigating to AI Test page: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private async void NewChatDialog()
        {
            try
            {
                // Show loading cursor while getting users
                Mouse.OverrideCursor = Cursors.Wait;
                
                // Get list of users
                var users = await _apiService.GetUsersAsync();
                
                // Restore cursor
                Mouse.OverrideCursor = null;
                
                // Filter out current user
                var otherUsers = users.Where(u => u.UserId != _apiService.CurrentUser?.UserId).ToList();
                
                Console.WriteLine($"Found {otherUsers.Count} users for chat selection (excluding current user)");
                
                if (otherUsers.Count == 0)
                {
                    MessageBox.Show("No other users found to chat with.", "New Chat", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                
                // Create a simple dialog
                var dialog = new Window
                {
                    Title = "New Chat",
                    Width = 400,
                    Height = 500,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                    Owner = Window.GetWindow(this),
                    ResizeMode = ResizeMode.NoResize,
                    WindowStyle = WindowStyle.ToolWindow,
                    Background = (SolidColorBrush)Application.Current.Resources["PrimaryBackgroundBrush"]
                };
                
                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(20)
                };
                
                var headerText = new TextBlock
                {
                    Text = "Select a user to chat with:",
                    FontSize = 16,
                    Margin = new Thickness(0, 0, 0, 20),
                    Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryTextBrush"]
                };
                
                var userListBox = new ListBox
                {
                    Height = 300,
                    Margin = new Thickness(0, 0, 0, 20),
                    Background = (SolidColorBrush)Application.Current.Resources["SecondaryBackgroundBrush"],
                    BorderThickness = new Thickness(1),
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderBrush"]
                };
                
                // Add users to the list box with additional info
                foreach (var user in otherUsers)
                {
                    // Create a more informative user list item
                    var userPanel = new StackPanel 
                    { 
                        Orientation = Orientation.Horizontal 
                    };
                    
                    // Create avatar
                    var avatarBorder = new Border
                    {
                        Width = 32,
                        Height = 32,
                        CornerRadius = new CornerRadius(16),
                        Background = (SolidColorBrush)Application.Current.Resources["TertiaryBackgroundBrush"],
                        Margin = new Thickness(0, 0, 10, 0)
                    };
                    
                    // Get first character of username for avatar
                    string firstChar = "?";
                    if (!string.IsNullOrEmpty(user.Username))
                    {
                        firstChar = user.Username.Substring(0, 1).ToUpper();
                    }
                    
                    var avatarText = new TextBlock
                    {
                        Text = firstChar,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryGreenBrush"],
                        FontWeight = FontWeights.SemiBold
                    };
                    
                    avatarBorder.Child = avatarText;
                    userPanel.Children.Add(avatarBorder);
                    
                    // Create user info
                    var userInfo = new StackPanel();
                    var nameText = new TextBlock
                    {
                        Text = user.Username ?? $"User {user.UserId}",
                        Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryTextBrush"],
                        FontWeight = FontWeights.SemiBold
                    };
                    
                    var emailText = new TextBlock
                    {
                        Text = user.Email ?? "",
                        Foreground = (SolidColorBrush)Application.Current.Resources["SecondaryTextBrush"],
                        FontSize = 12
                    };
                    
                    userInfo.Children.Add(nameText);
                    userInfo.Children.Add(emailText);
                    userPanel.Children.Add(userInfo);
                    
                    // Add to list box
                    var item = new ListBoxItem
                    {
                        Content = userPanel,
                        Tag = user.UserId,
                        Padding = new Thickness(8),
                        Margin = new Thickness(0, 2, 0, 2)
                    };
                    
                    userListBox.Items.Add(item);
                }
                
                var buttonsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                
                var cancelButton = new Button
                {
                    Content = "Cancel",
                    Width = 100,
                    Height = 30,
                    Margin = new Thickness(0, 0, 10, 0),
                    Background = Brushes.Transparent,
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["BorderBrush"],
                    Foreground = (SolidColorBrush)Application.Current.Resources["PrimaryTextBrush"]
                };
                
                var startChatButton = new Button
                {
                    Content = "Start Chat",
                    Width = 100,
                    Height = 30,
                    IsEnabled = false,
                    Background = (SolidColorBrush)Application.Current.Resources["PrimaryGreenBrush"],
                    Foreground = Brushes.White
                };
                
                // Enable start chat button only when a user is selected
                userListBox.SelectionChanged += (s, e) =>
                {
                    startChatButton.IsEnabled = userListBox.SelectedItem != null;
                };
                
                // Set up button click handlers
                cancelButton.Click += (s, e) => dialog.Close();
                
                startChatButton.Click += async (s, e) =>
                {
                    if (userListBox.SelectedItem is ListBoxItem selectedItem)
                    {
                        int userId = (int)selectedItem.Tag;
                        string username = "User";
                        
                        // Extract the username from the content if possible
                        if (selectedItem.Content is StackPanel panel && 
                            panel.Children.Count > 1 && 
                            panel.Children[1] is StackPanel infoPanel &&
                            infoPanel.Children.Count > 0 &&
                            infoPanel.Children[0] is TextBlock nameBlock)
                        {
                            username = nameBlock.Text;
                        }
                        
                        // Check if a thread already exists with this user
                        bool shouldCreateNewThread = true;
                        string existingThreadId = null!;
                        
                        try
                        {
                            // Show loading indicator
                            Mouse.OverrideCursor = Cursors.Wait;
                            
                            // Check existing threads to see if there's already a conversation with this user
                            var threads = await _apiService.GetThreadsAsync();
                            
                            foreach (var thread in threads)
                            {
                                var participants = await _apiService.GetThreadParticipantsAsync(thread.ThreadId);
                                
                                if (participants != null && participants.Count >= 2)
                                {
                                    // Check if this thread has both the current user and the selected user
                                    bool containsCurrentUser = participants.Any(p => p.UserId == _apiService.CurrentUser?.UserId);
                                    bool containsSelectedUser = participants.Any(p => p.UserId == userId);
                                    
                                    if (containsCurrentUser && containsSelectedUser)
                                    {
                                        // Found an existing thread with this user
                                        existingThreadId = thread.ThreadId.ToString();
                                        
                                        // Only navigate to the thread if it's not closed
                                        if (!thread.IsClosed)
                                        {
                                            // Use existing thread instead of creating a new one
                                            shouldCreateNewThread = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            
                            dialog.Close();
                            
                            if (shouldCreateNewThread)
                            {
                            // Create private chat with selected user
                            var thread = await _apiService.CreatePrivateChatAsync(userId, $"Chat with {username}");
                            
                            if (thread != null && thread.ThreadId > 0)
                            {
                                // Reload the threads to show the new chat
                                await LoadChatThreads();
                                
                                // Select the new thread
                                LoadConversation(thread.ThreadId.ToString());
                            }
                            else
                            {
                                MessageBox.Show("Failed to create new chat.", "Error", 
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            else if (!string.IsNullOrEmpty(existingThreadId))
                            {
                                // Navigate to the existing thread
                                await LoadChatThreads();
                                LoadConversation(existingThreadId);
                                
                                // Notify the user we're using an existing thread
                                MessageBox.Show($"A conversation with {username} already exists. Opening the existing chat.",
                                    "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Error creating chat: {ex.Message}", "Error", 
                                MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            Mouse.OverrideCursor = null;
                        }
                    }
                };
                
                // Add all controls to the main stack panel
                buttonsPanel.Children.Add(cancelButton);
                buttonsPanel.Children.Add(startChatButton);
                
                stackPanel.Children.Add(headerText);
                stackPanel.Children.Add(userListBox);
                stackPanel.Children.Add(buttonsPanel);
                
                dialog.Content = stackPanel;
                dialog.ShowDialog();
            }
            catch (Exception ex)
            {
                Mouse.OverrideCursor = null;
                MessageBox.Show($"Error opening user selection: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Console.WriteLine($"Error in NewChatDialog: {ex.Message}\n{ex.StackTrace}");
            }
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

        private void MessageBubble_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is ChatMessageViewModel message)
            {
                // Only toggle censorship for messages that contain profanity
                if (message.ContainsProfanity)
                {
                    Console.WriteLine($"Message clicked. Contains profanity: {message.ContainsProfanity}, Is currently uncensored: {message.IsUncensored}");
                    message.ToggleCensorship();
                    e.Handled = true;
                }
            }
        }

        private void ShowSwearingWarning(int score)
        {
            // Run on UI thread
            Dispatcher.Invoke(() =>
            {
                try
                {
                    // Configure warning content based on severity
                    string warningText;
                    if (score <= 2)
                    {
                        warningText = "Mild inappropriate language detected. Please be respectful.";
                    }
                    else if (score <= 4)
                    {
                        warningText = "Repeated inappropriate language detected. Further violations may close this chat.";
                    }
                    else
                    {
                        warningText = "Final warning: Excessive inappropriate language. One more violation will close this chat.";
                    }

                    // Update the warning text and display it
                    if (SwearingWarningText != null)
                    {
                        SwearingWarningText.Text = warningText;
                    }

                    if (SwearingWarningBorder != null)
                    {
                        SwearingWarningBorder.Visibility = Visibility.Visible;
                        
                        // Create a timer to hide the warning after a few seconds
                        var hideTimer = new System.Timers.Timer(5000); // 5 seconds
                        hideTimer.AutoReset = false;
                        hideTimer.Elapsed += (s, e) => 
                        {
                            Dispatcher.Invoke(() => 
                            {
                                SwearingWarningBorder.Visibility = Visibility.Collapsed;
                            });
                            hideTimer.Dispose();
                        };
                        hideTimer.Start();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error showing swearing warning: {ex.Message}");
                }
            });
        }

        private async Task CloseThreadDueToExcessiveSwearing()
        {
            try
            {
                Console.WriteLine("Closing thread due to excessive swearing");
                
                // Set the thread as closed locally
                IsThreadClosed = true;
                
                // Notify users
                Messages.Add(new ChatMessageViewModel
                {
                    IsSent = false,
                    Text = "⚠️ This conversation has been closed due to excessive swearing. You can no longer send messages.",
                    Timestamp = DateTime.Now.ToString("h:mm tt"),
                    Avatar = "🔒"
                });
                
                // Scroll to show the notification
                ScrollToBottom();
                
                // Add a delay to ensure UI updates
                await Task.Delay(100);
                
                // Update the thread on the server to mark it as closed
                await _apiService.CloseThreadAsync(_currentThreadId);
                
                // Show a message box to inform the user
                await Dispatcher.InvokeAsync(() => {
                    MessageBox.Show(
                        "This conversation has been closed due to excessive swearing. Messages can no longer be sent.",
                        "Conversation Closed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error closing thread: {ex}");
                
                // Try to log the error to a file
                await Task.Run(() => {
                    try {
                        // Simple error logging
                        File.AppendAllText(
                            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log"), 
                            $"{DateTime.Now}: Error closing thread: {ex}\r\n");
                    }
                    catch {
                        // Ignore errors in error handling
                    }
                });
            }
        }
        
        protected void OnPropertyChanged(string propertyName)
        {
            Console.WriteLine($"Property changed: {propertyName}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void ConversationItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string threadId)
            {
                Console.WriteLine($"[CLICK] Conversation clicked: {threadId}");
                
                // If we're already in a thread, leave that group first
                if (_currentThreadId > 0 && int.TryParse(threadId, out int newThreadId) && _currentThreadId != newThreadId)
                {
                    try
                    {
                        Console.WriteLine($"[CLICK] Leaving previous thread group {_currentThreadId} before joining new one...");
                        bool leaveResult = await _apiService.LeaveThreadChatGroupAsync(_currentThreadId);
                        Console.WriteLine($"[CLICK] Leave thread result: {leaveResult}");
                        
                        // Small delay to ensure server processes the leave request
                        await Task.Delay(300);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[CLICK ERROR] Error leaving previous thread group: {ex.Message}");
                        // Continue even if there's an error
                    }
                }
                
                // Now load the new conversation
                LoadConversation(threadId);
                e.Handled = true;
            }
            else
            {
                Console.WriteLine($"[CLICK WARNING] Conversation clicked but tag is not a string: {(sender as FrameworkElement)?.Tag}");
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
                
                Console.WriteLine($"[CONVERSATION] Loading thread: {threadId}");
                _currentThreadId = parsedThreadId;
                
                // Set up UI immediately to provide user feedback
                ConversationItemViewModel? selectedConversation = null;
                foreach (var conv in Conversations)
                {
                    conv.IsSelected = conv.Id == threadId;
                    
                    // Reset unread count for the selected conversation
                    if (conv.Id == threadId)
                    {
                        conv.UnreadCount = 0;
                        selectedConversation = conv;
                    }
                }
                
                // Get the selected conversation from the list
                var conversation = Conversations.FirstOrDefault(c => c.Id == threadId);
                if (conversation != null)
                {
                    // Set the current contact info
                    CurrentContactName = conversation.Title;
                    OnPropertyChanged(nameof(CurrentContactName));
                    
                    // Update contact header UI elements
                    if (ContactNameDisplay != null)
                    {
                        ContactNameDisplay.Text = CurrentContactName;
                    }
                    
                    if (ContactAvatarText != null && !string.IsNullOrEmpty(CurrentContactName))
                    {
                        ContactAvatarText.Text = CurrentContactName.Length > 0 ? 
                            CurrentContactName[0].ToString().ToUpper() : "?";
                    }
                    
                    // Set HasSelectedConversation to true before loading messages
                    HasSelectedConversation = true;
                    
                    // IMPORTANT: Set swearing score and thread closed status from the conversation
                    // Add detailed logging to verify values
                    Console.WriteLine($"[CONVERSATION] Thread data - SwearingScore: {conversation.SwearingScore}, IsClosed: {conversation.IsClosed}");
                    
                    // Forcefully set the swearing score to the proper value
                    _swearingScore = conversation.SwearingScore ?? 0;
                    OnPropertyChanged(nameof(SwearingScore));
                    
                    IsThreadClosed = conversation.IsClosed;
                    
                    // Force UI refresh for swearing score and thread status
                    RefreshUIElements();
                    
                    // Immediately poll for the latest swearing score from server
                    Task.Run(async () => {
                        try {
                            int score = await _apiService.GetThreadSwearingScoreAsync(parsedThreadId);
                            bool isClosed = await _apiService.IsThreadClosedAsync(parsedThreadId);
                            
                            Console.WriteLine($"[CONVERSATION] Latest server data - SwearingScore: {score}, IsClosed: {isClosed}");
                            
                            // Update on dispatcher thread
                            await Dispatcher.InvokeAsync(() => {
                                SwearingScore = score;
                                IsThreadClosed = isClosed;
                                RefreshUIElements();
                            });
                        }
                        catch (Exception pollEx) {
                            Console.WriteLine($"[CONVERSATION] Error polling for latest score: {pollEx.Message}");
                        }
                    });
                    
                    Console.WriteLine($"[CONVERSATION] Updated UI properties - SwearingScore: {_swearingScore}, IsThreadClosed: {IsThreadClosed}");
                }
                
                // Clear existing messages before loading new ones
                Messages.Clear();
                
                // Force immediate UI refresh - don't wait for data
                Dispatcher.Invoke(() => {
                    Console.WriteLine("[CONVERSATION] Forcing UI update for chat view");
                    UpdateLayout(); // Force layout update
                });
                
                // Temporarily unsubscribe from SignalR message events to avoid duplicate messages during loading
                Console.WriteLine("[CONVERSATION] Temporarily unsubscribing from message events");
                _apiService.OnMessageReceived -= HandleMessageReceived;
                
                // First make sure we're connected to SignalR
                bool isConnected = await _apiService.IsHubConnectedAsync();
                if (!isConnected)
                {
                    Console.WriteLine("[CONVERSATION] SignalR not connected, attempting to connect...");
                    bool connectionResult = await _apiService.ConnectToHubAsync();
                    Console.WriteLine($"[CONVERSATION] Connection attempt result: {connectionResult}");
                    
                    // Update connection status in UI
                    IsConnected = await _apiService.IsHubConnectedAsync();
                    UpdateConnectionStatus();
                }
                
                // Load messages for the selected thread
                Console.WriteLine($"[CONVERSATION] Loading messages for thread {threadId}...");
                var messages = await _apiService.GetMessagesAsync(parsedThreadId);
                Console.WriteLine($"[CONVERSATION] Retrieved {messages?.Count ?? 0} messages from API");
                
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
                            OriginalText = message.OriginalMessage,
                            Timestamp = message.CreatedAt.ToString("h:mm tt"),
                            Avatar = avatar,
                            ContainsProfanity = message.WasModified,
                            IsUncensored = false // Initially show censored version
                        });
                    }
                    
                    // After adding all messages, scroll to bottom
                    UpdateLayout(); // Force layout update again
                ScrollToBottom();
                }
                else
                {
                    Console.WriteLine("[CONVERSATION] No messages found for this thread");
                }
                
                // IMPORTANT: Explicitly join the thread's SignalR group AFTER loading messages
                try
                {
                    Console.WriteLine($"[CONVERSATION] Joining SignalR group for thread {parsedThreadId}...");
                    bool joinResult = await _apiService.JoinThreadChatGroupAsync(parsedThreadId);
                    Console.WriteLine($"[CONVERSATION] Join result: {joinResult}");
                    
                    // Re-check connection status after join attempt
                    IsConnected = await _apiService.IsHubConnectedAsync();
                    UpdateConnectionStatus();
                    
                    // Re-subscribe to message events now that history is loaded
                    Console.WriteLine("[CONVERSATION] Re-subscribing to message events");
                    _apiService.OnMessageReceived += HandleMessageReceived;
            }
            catch (Exception ex)
            {
                    Console.WriteLine($"[CONVERSATION ERROR] Error joining thread SignalR group: {ex.Message}");
                    // Make sure to re-subscribe even if joining the group fails
                    _apiService.OnMessageReceived += HandleMessageReceived;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CONVERSATION ERROR] Error loading thread messages: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show($"Error loading messages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                
                // Make sure to re-subscribe in case of errors
                _apiService.OnMessageReceived += HandleMessageReceived;
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                SendMessage();
                e.Handled = true; // Prevent default Enter behavior (new line)
            }
        }

        private void StartScorePolling()
        {
            StopScorePolling();
            
            _scorePollingTimer = new System.Timers.Timer(SCORE_POLLING_INTERVAL_MS);
            _scorePollingTimer.Elapsed += async (sender, e) => 
            {
                try
                {
                    if (_currentThreadId <= 0 || !IsConnected)
                        return;
                    
                    // Poll for score and closed status
                    int score = await _apiService.GetThreadSwearingScoreAsync(_currentThreadId);
                    bool isClosed = await _apiService.IsThreadClosedAsync(_currentThreadId);
                    
                    Console.WriteLine($"[POLL] Thread {_currentThreadId}: Score={score}, IsClosed={isClosed}");
                    
                    // Only update if different from current values
                    if (score != SwearingScore || isClosed != IsThreadClosed)
                    {
                        Console.WriteLine($"[POLL] Values changed - Old score: {SwearingScore}, New score: {score}, Old closed: {IsThreadClosed}, New closed: {isClosed}");
                        
                        // Update in UI thread
                        Dispatcher.Invoke(() => 
                        {
                            SwearingScore = score;
                            IsThreadClosed = isClosed;
                            RefreshUIElements();
                            
                            // If newly closed, add a system message
                            if (isClosed && !IsThreadClosed)
                            {
                                Messages.Add(new ChatMessageViewModel
                                {
                                    IsSent = false,
                                    Text = "⚠️ This conversation has been closed due to excessive swearing.",
                                    Timestamp = DateTime.Now.ToString("h:mm tt"),
                                    Avatar = "🔒"
                                });
                                
                                ScrollToBottom();
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[POLL ERROR] {ex.Message}");
                }
            };
            
            _scorePollingTimer.Start();
            Console.WriteLine("[POLL] Started score polling timer");
        }
        
        private void StopScorePolling()
        {
            if (_scorePollingTimer != null)
            {
                _scorePollingTimer.Stop();
                _scorePollingTimer.Dispose();
                _scorePollingTimer = null;
                Console.WriteLine("[POLL] Stopped score polling timer");
            }
        }
    }
} 