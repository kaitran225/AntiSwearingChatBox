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
        public event EventHandler MenuRequested;
        public event EventHandler AttachmentRequested;

        #endregion

        #region Event Handlers

        private void MessageInput_MessageSent(object sender, string message)
        {
            // Notify parent
            MessageSent?.Invoke(this, message);
        }

        private void MessageInput_AttachmentRequested(object sender, EventArgs e)
        {
            AttachmentRequested?.Invoke(this, e);
        }

        private void Header_MenuRequested(object sender, EventArgs e)
        {
            MenuRequested?.Invoke(this, e);
        }

        #endregion

        #region Methods

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
} 