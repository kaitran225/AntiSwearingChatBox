using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AntiSwearingChatBox.App.Components
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ChatView()
        {
            InitializeComponent();
            this.DataContext = this;
            Messages = new ObservableCollection<MessageViewModel>();
        }

        #region Properties

        private ObservableCollection<MessageViewModel> _messages;
        public ObservableCollection<MessageViewModel> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                OnPropertyChanged(nameof(Messages));
            }
        }

        private ContactViewModel _currentContact;
        public ContactViewModel CurrentContact
        {
            get { return _currentContact; }
            set
            {
                _currentContact = value;
                OnPropertyChanged(nameof(CurrentContact));
            }
        }

        #endregion

        #region Events

        public event EventHandler<string> MessageSent;
        public event EventHandler PhoneCallRequested;
        public event EventHandler VideoCallRequested;
        public event EventHandler MenuRequested;
        public event EventHandler AttachmentRequested;

        #endregion

        #region Event Handlers

        private void MessageInput_MessageSent(object sender, string message)
        {
            // Add sent message to the list
            var newMessage = new MessageViewModel
            {
                IsSent = true,
                Text = message,
                Timestamp = DateTime.Now.ToString("h:mm tt"),
                Avatar = "ME",
                Background = Application.Current.Resources["PrimaryBackgroundBrush"] as SolidColorBrush,
                BorderBrush = Application.Current.Resources["PrimaryGreenBrush"] as SolidColorBrush
            };

            Messages.Add(newMessage);
            ScrollToBottom();

            // Notify parent
            MessageSent?.Invoke(this, message);
        }

        private void MessageInput_AttachmentRequested(object sender, EventArgs e)
        {
            AttachmentRequested?.Invoke(this, e);
        }

        private void Header_PhoneCallRequested(object sender, EventArgs e)
        {
            PhoneCallRequested?.Invoke(this, e);
        }

        private void Header_VideoCallRequested(object sender, EventArgs e)
        {
            VideoCallRequested?.Invoke(this, e);
        }

        private void Header_MenuRequested(object sender, EventArgs e)
        {
            MenuRequested?.Invoke(this, e);
        }

        #endregion

        #region Methods

        public void AddReceivedMessage(string message, string sender, string avatar = null)
        {
            var newMessage = new MessageViewModel
            {
                IsSent = false,
                Text = message,
                Timestamp = DateTime.Now.ToString("h:mm tt"),
                Avatar = avatar ?? sender?.Substring(0, 2).ToUpper() ?? "??",
                Background = Application.Current.Resources["SecondaryBackgroundBrush"] as SolidColorBrush,
                BorderBrush = Application.Current.Resources["BorderBrush"] as SolidColorBrush
            };

            Messages.Add(newMessage);
            ScrollToBottom();
        }

        private void ScrollToBottom()
        {
            // Wait for UI to update before scrolling
            this.Dispatcher.InvokeAsync(() =>
            {
                MessageScrollViewer.ScrollToEnd();
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    public class MessageViewModel
    {
        public bool IsSent { get; set; }
        public string Text { get; set; }
        public string Timestamp { get; set; }
        public string Avatar { get; set; }
        public Brush Background { get; set; }
        public Brush BorderBrush { get; set; }
    }

    public class ContactViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Initials { get; set; }
        public string Status { get; set; }
        public string LastMessage { get; set; }
        public string LastMessageTime { get; set; }
        public bool IsOnline { get; set; }
        public bool IsActive { get; set; }
        public bool HasUnread { get; set; }
        public int UnreadCount { get; set; }
        public Visibility Visibility { get; set; } = Visibility.Visible;
    }
} 