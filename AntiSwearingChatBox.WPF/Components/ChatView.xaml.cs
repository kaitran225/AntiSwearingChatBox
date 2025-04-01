using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AntiSwearingChatBox.WPF.Components
{
    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<string> MessageSent;
        public event EventHandler MenuRequested;
        public event EventHandler AttachmentRequested;

        public ObservableCollection<ChatMessageViewModel> Messages { get; private set; }

        public ChatView()
        {
            InitializeComponent();
            this.DataContext = this;
            Messages = new ObservableCollection<ChatMessageViewModel>();
            MessagesList.ItemsSource = Messages;
        }

        #region Properties

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

        #region Event Handlers

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            MessageSent?.Invoke(this, MessageTextBox.Text);
            MessageTextBox.Clear();
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            MenuRequested?.Invoke(this, EventArgs.Empty);
        }

        private void AttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            AttachmentRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Methods

        private void ScrollToBottom()
        {
            // Wait for UI to update before scrolling
            this.Dispatcher.InvokeAsync(() =>
            {
                MessagesScroll.ScrollToEnd();
            }, System.Windows.Threading.DispatcherPriority.ContextIdle);
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 