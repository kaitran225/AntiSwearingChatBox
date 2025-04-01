using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AntiSwearingChatBox.WPF.Commands;
using AntiSwearingChatBox.WPF.Models.Api;
using AntiSwearingChatBox.WPF.Services;

namespace AntiSwearingChatBox.WPF.ViewModels
{
    public class ChatViewModel : ViewModelBase
    {
        private string? _messageText = string.Empty;
        private string? _currentUser = string.Empty;
        private ObservableCollection<ChatMessage>? _messages;
        private ObservableCollection<ChatThread>? _threads;
        private ChatThread? _selectedThread;
        private bool _isConnected;
        private bool _isLoading;
        
        public string MessageText
        {
            get => _messageText!;
            set => SetProperty(ref _messageText, value);
        }
        
        public string CurrentUser
        {
            get => _currentUser!;
            set => SetProperty(ref _currentUser, value);
        }
        
        public ObservableCollection<ChatMessage> Messages
        {
            get => _messages!;
            set => SetProperty(ref _messages, value);
        }
        
        public ObservableCollection<ChatThread> Threads
        {
            get => _threads!;
            set => SetProperty(ref _threads, value);
        }
        
        public ChatThread SelectedThread
        {
            get => _selectedThread!;
            set
            {
                if (SetProperty(ref _selectedThread, value) && value != null)
                {
                    // Load messages for the selected thread
                    Task.Run(async () => await LoadMessagesAsync(value.ThreadId));
                }
            }
        }
        
        public bool IsConnected
        {
            get => _isConnected;
            set => SetProperty(ref _isConnected, value);
        }
        
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        
        public ICommand SendMessageCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand CreateThreadCommand { get; }
        public ICommand RefreshThreadsCommand { get; }
        
        public ChatViewModel(string username)
        {
            CurrentUser = username;
            Messages = new ObservableCollection<ChatMessage>();
            Threads = new ObservableCollection<ChatThread>();
            
            SendMessageCommand = new RelayCommand(ExecuteSendMessageAsync, CanSendMessage);
            LogoutCommand = new RelayCommand(ExecuteLogout);
            CreateThreadCommand = new RelayCommand(ExecuteCreateThreadAsync);
            RefreshThreadsCommand = new RelayCommand(ExecuteRefreshThreadsAsync);
            
            // Setup event handlers for real-time updates
            ServiceProvider.ApiService.OnMessageReceived += OnMessageReceived;
            ServiceProvider.ApiService.OnThreadCreated += OnThreadCreated;
            
            // Connect to SignalR hub and load threads
            InitializeAsync();
        }
        
        private async void InitializeAsync()
        {
            try
            {
                IsLoading = true;
                await ServiceProvider.ApiService.ConnectToHubAsync();
                IsConnected = true;
                await LoadThreadsAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect: {ex.Message}", "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task LoadThreadsAsync()
        {
            try
            {
                IsLoading = true;
                var threads = await ServiceProvider.ApiService.GetThreadsAsync();
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Threads.Clear();
                    foreach (var thread in threads)
                    {
                        Threads.Add(thread);
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load threads: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task LoadMessagesAsync(int threadId)
        {
            try
            {
                IsLoading = true;
                var messages = await ServiceProvider.ApiService.GetMessagesAsync(threadId);
                
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Clear();
                    
                    if (messages != null && messages.Count > 0)
                    {
                        Console.WriteLine($"Adding {messages.Count} messages to the view");
                        
                        foreach (var message in messages)
                        {
                            Console.WriteLine($"Adding message: {message.Content} from {message.SenderName}");
                            Messages.Add(message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No messages to display");
                    }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load messages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private bool CanSendMessage()
        {
            return IsConnected && 
                   SelectedThread != null && 
                   !string.IsNullOrWhiteSpace(MessageText) && 
                   !IsLoading;
        }
        
        private async void ExecuteSendMessageAsync()
        {
            if (SelectedThread == null || string.IsNullOrWhiteSpace(MessageText))
                return;

            try
            {
                IsLoading = true;
                var sentMessage = await ServiceProvider.ApiService.SendMessageAsync(SelectedThread.ThreadId, MessageText);
                
                // Add the sent message to the Messages collection
                if (sentMessage != null && sentMessage.MessageId > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Messages.Add(sentMessage);
                        Console.WriteLine($"Added sent message to UI: {sentMessage.Content}");
                    });
                }
                
                MessageText = string.Empty;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to send message: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async void ExecuteCreateThreadAsync()
        {
            string threadName = string.Empty;
            
            // Simple input dialog
            try {
                threadName = Microsoft.VisualBasic.Interaction.InputBox("Enter thread name:", "Create Thread", "");
            }
            catch (Exception ex) {
                // Fallback if Microsoft.VisualBasic is not available
                Console.WriteLine($"Failed to display input dialog: {ex.Message}");
                threadName = "New Thread";
                MessageBox.Show("Could not display input dialog. Created default thread name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            
            if (string.IsNullOrWhiteSpace(threadName))
                return;
                
            try
            {
                IsLoading = true;
                await ServiceProvider.ApiService.CreateThreadAsync(threadName);
                // The new thread will be added through the OnThreadCreated event handler
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create thread: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async void ExecuteRefreshThreadsAsync()
        {
            await LoadThreadsAsync();
        }
        
        private void ExecuteLogout()
        {
            try
            {
                ServiceProvider.ApiService.DisconnectFromHubAsync().ConfigureAwait(false);
                // Navigate back to login
                // You'll need to implement navigation logic here
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during logout: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void OnMessageReceived(ChatMessage message)
        {
            if (SelectedThread != null && message.ThreadId == SelectedThread.ThreadId)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Messages.Add(message);
                });
            }
        }
        
        private void OnThreadCreated(ChatThread thread)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Threads.Add(thread);
            });
        }
    }
} 