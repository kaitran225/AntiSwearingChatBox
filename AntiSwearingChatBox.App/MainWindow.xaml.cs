using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;
using Anti_Swearing_Chat_Box.Core.Models;
using Anti_Swearing_Chat_Box.Core.Services;
using Anti_Swearing_Chat_Box.Core.Converters;

namespace Anti_Swearing_Chat_Box.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private readonly IAIModerationService _aiModerationService;
        private readonly IValueConverter _boolToStatusConverter;
        private readonly IValueConverter _boolToStatusColorConverter;
        private readonly IValueConverter _boolToMessageBackgroundConverter;
        private readonly IValueConverter _boolToHorizontalAlignmentConverter;
        private readonly IValueConverter _intToVisibilityConverter;

        private ObservableCollection<Contact> _contacts;
        private ObservableCollection<ChatThread> _chatThreads;
        private Contact _currentContact;
        private ChatThread _currentChatThread;

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Contact> Contacts
        {
            get => _contacts;
            set
            {
                _contacts = value;
                OnPropertyChanged(nameof(Contacts));
            }
        }

        public ObservableCollection<ChatThread> ChatThreads
        {
            get => _chatThreads;
            set
            {
                _chatThreads = value;
                OnPropertyChanged(nameof(ChatThreads));
            }
        }

        public Contact CurrentContact
        {
            get => _currentContact;
            set
            {
                _currentContact = value;
                OnPropertyChanged(nameof(CurrentContact));
            }
        }

        public ChatThread CurrentChatThread
        {
            get => _currentChatThread;
            set
            {
                _currentChatThread = value;
                OnPropertyChanged(nameof(CurrentChatThread));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            _aiModerationService = new AIModerationService();
            _boolToStatusConverter = new BoolToStatusConverter();
            _boolToStatusColorConverter = new BoolToStatusColorConverter();
            _boolToMessageBackgroundConverter = new BoolToMessageBackgroundConverter();
            _boolToHorizontalAlignmentConverter = new BoolToHorizontalAlignmentConverter();
            _intToVisibilityConverter = new IntToVisibilityConverter();

            InitializeMockData();
        }

        private void InitializeMockData()
        {
            // Initialize contacts
            Contacts = new ObservableCollection<Contact>
            {
                new Contact { Id = 1, DisplayName = "John Doe", Initials = "JD", IsOnline = true, LastSeen = DateTime.Now },
                new Contact { Id = 2, DisplayName = "Jane Smith", Initials = "JS", IsOnline = false, LastSeen = DateTime.Now.AddHours(-1) },
                new Contact { Id = 3, DisplayName = "Mike Johnson", Initials = "MJ", IsOnline = true, LastSeen = DateTime.Now }
            };

            // Initialize chat threads
            ChatThreads = new ObservableCollection<ChatThread>
            {
                new ChatThread 
                { 
                    Id = 1, 
                    Contact = Contacts[0],
                    LastMessageText = "Hey, how are you?",
                    LastMessageTime = DateTime.Now.AddMinutes(-5),
                    UnreadCount = 2
                },
                new ChatThread 
                { 
                    Id = 2, 
                    Contact = Contacts[1],
                    LastMessageText = "See you tomorrow!",
                    LastMessageTime = DateTime.Now.AddHours(-1),
                    UnreadCount = 0
                },
                new ChatThread 
                { 
                    Id = 3, 
                    Contact = Contacts[2],
                    LastMessageText = "Great idea!",
                    LastMessageTime = DateTime.Now.AddMinutes(-15),
                    UnreadCount = 1
                }
            };

            // Set current contact and chat thread
            CurrentContact = Contacts[0];
            CurrentChatThread = ChatThreads[0];
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (WindowState == WindowState.Maximized)
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
                WindowState = WindowState.Normal;
            else
                WindowState = WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ChatsTabButton_Click(object sender, RoutedEventArgs e)
        {
            ChatsTabButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            ChatsTabButton.FontWeight = FontWeights.Bold;
            ContactsTabButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"));
            ContactsTabButton.FontWeight = FontWeights.Normal;

            ChatsListView.Visibility = Visibility.Visible;
            ContactsListView.Visibility = Visibility.Collapsed;
        }

        private void ContactsTabButton_Click(object sender, RoutedEventArgs e)
        {
            ContactsTabButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
            ContactsTabButton.FontWeight = FontWeights.Bold;
            ChatsTabButton.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#757575"));
            ChatsTabButton.FontWeight = FontWeights.Normal;

            ContactsListView.Visibility = Visibility.Visible;
            ChatsListView.Visibility = Visibility.Collapsed;
        }

        private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            var message = MessageTextBox.Text;
            MessageTextBox.Clear();

            // Check for inappropriate content
            var isAppropriate = await _aiModerationService.IsAppropriateContent(message);
            if (!isAppropriate)
            {
                MessageBox.Show("Your message contains inappropriate content and cannot be sent.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Add message to current chat thread
            var newMessage = new Message
            {
                Id = CurrentChatThread.Messages.Count + 1,
                Content = message,
                Timestamp = DateTime.Now,
                IsFromCurrentUser = true
            };

            CurrentChatThread.Messages.Add(newMessage);
            CurrentChatThread.LastMessageText = message;
            CurrentChatThread.LastMessageTime = DateTime.Now;
            CurrentChatThread.UnreadCount = 0;

            // Simulate response
            await System.Threading.Tasks.Task.Delay(1000);
            var response = new Message
            {
                Id = CurrentChatThread.Messages.Count + 1,
                Content = "Thanks for your message!",
                Timestamp = DateTime.Now,
                IsFromCurrentUser = false
            };

            CurrentChatThread.Messages.Add(response);
            CurrentChatThread.LastMessageText = response.Content;
            CurrentChatThread.LastMessageTime = DateTime.Now;
            CurrentChatThread.UnreadCount = 1;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}