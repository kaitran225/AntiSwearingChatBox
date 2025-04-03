using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AntiSwearingChatBox.WPF.Utilities;
using System.Linq;
using AntiSwearingChatBox.WPF.ViewModels;

namespace AntiSwearingChatBox.WPF.Components
{
    /// <summary>
    /// Interaction logic for ConversationList.xaml
    /// </summary>
    public partial class ConversationList : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public ConversationList()
        {
            InitializeComponent();
            this.DataContext = this;
            
            // Initialize collections
            Conversations = new ObservableCollection<ConversationItemViewModel>();
            Groups = new ObservableCollection<ConversationItemViewModel>();
            
            // Hook up event handlers
            AdminDashboardButton.Click += AdminDashboard_Click;
            
            // Subscribe to collection changes
            Conversations.CollectionChanged += (s, args) =>
            {
                // When items are added, make sure they're connected
                RefreshItemEventHandlers();
            };
        }

        #region Properties

        private string _searchText = string.Empty;
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                FilterConversations();
            }
        }

        private ObservableCollection<ConversationItemViewModel> _allConversations = new();
        private ObservableCollection<ConversationItemViewModel> _filteredConversations = new();
        private ObservableCollection<ConversationItemViewModel>? _conversations;
        public ObservableCollection<ConversationItemViewModel> Conversations
        {
            get { return _conversations!; }
            set
            {
                _conversations = value;
                OnPropertyChanged(nameof(Conversations));
            }
        }

        private ObservableCollection<ConversationItemViewModel>? _groups;
        public ObservableCollection<ConversationItemViewModel> Groups
        {
            get { return _groups!; }
            set
            {
                _groups = value;
                OnPropertyChanged(nameof(Groups));
                OnPropertyChanged(nameof(HasGroups));
            }
        }

        private bool _isAdmin;
        public bool IsAdmin
        {
            get { return _isAdmin; }
            set
            {
                _isAdmin = value;
                OnPropertyChanged(nameof(IsAdmin));
            }
        }

        public bool HasGroups => Groups?.Any() == true;

        #endregion

        #region Events

        public event EventHandler<string>? ConversationSelected;
        public event EventHandler? NewChatRequested;
        public event EventHandler? AddConversationRequested;
        public event EventHandler? AdminDashboardRequested;

        #endregion

        #region Event Handlers

        private void ConversationItem_Selected(object sender, string id)
        {
            // Notify parent about selection
            ConversationSelected?.Invoke(this, id);
        }
        
        private void ConversationItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ConversationItem item && item.Tag is string id)
            {
                ConversationSelected?.Invoke(this, id);
            }
        }

        private void NewChat_Click(object sender, RoutedEventArgs e)
        {
            NewChatRequested?.Invoke(this, EventArgs.Empty);
        }

        private void AddConversation_Click(object sender, RoutedEventArgs e)
        {
            AddConversationRequested?.Invoke(this, EventArgs.Empty);
        }
        
        private void AdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            AdminDashboardRequested?.Invoke(this, EventArgs.Empty);
        }
        
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                SearchText = textBox.Text;
            }
        }
        
        // Alias methods to maintain compatibility
        private void NewChatButton_Click(object sender, RoutedEventArgs e)
        {
            NewChat_Click(sender, e);
        }

        private void AddConversationButton_Click(object sender, RoutedEventArgs e)
        {
            AddConversation_Click(sender, e);
        }
        
        private void AdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            AdminDashboard_Click(sender, e);
        }

        #endregion

        #region Methods

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SelectConversation(ConversationItemViewModel conversation)
        {
            // UI method to highlight the selected conversation
        }

        public void AddConversation(ConversationItemViewModel conversation)
        {
            _allConversations.Add(conversation);
            FilterConversations();
        }
        
        public void AddConversation(string id, string title, string lastMessage, string lastMessageTime)
        {
            var conversation = new ConversationItemViewModel
            {
                Id = id,
                Title = title,
                LastMessage = lastMessage,
                LastMessageTime = lastMessageTime,
                UnreadCount = 0,
                IsSelected = false
            };
            
            _allConversations.Add(conversation);
            FilterConversations();
        }

        public void AddGroup(ConversationItemViewModel group)
        {
            Groups.Add(group);
            OnPropertyChanged(nameof(HasGroups));
        }

        public void UpdateConversation(string id, string lastMessage, string timestamp, bool increaseUnread = false)
        {
            var conversation = _allConversations.FirstOrDefault(c => c.Id == id);
            if (conversation != null)
            {
                conversation.LastMessage = lastMessage;
                conversation.LastMessageTime = timestamp;
                conversation.MessageStatus = MessageStatus.Sent;

                if (increaseUnread && !conversation.IsSelected)
                {
                    conversation.UnreadCount++;
                }
            }
        }

        public void UpdateMessageStatus(string id, MessageStatus status)
        {
            var conversation = _allConversations.FirstOrDefault(c => c.Id == id);
            if (conversation != null)
            {
                conversation.MessageStatus = status;
            }
        }

        public void UpdateOnlineStatus(string id, bool isOnline, string? lastSeen = null)
        {
            var conversation = _allConversations.FirstOrDefault(c => c.Id == id);
            if (conversation != null)
            {
                conversation.IsOnline = isOnline;
                if (!isOnline && !string.IsNullOrEmpty(lastSeen))
                {
                    conversation.LastSeen = lastSeen;
                    conversation.ShowLastSeen = true;
                }
                else
                {
                    conversation.ShowLastSeen = false;
                }
            }
        }

        public void SetTyping(string id, bool isTyping)
        {
            var conversation = _allConversations.FirstOrDefault(c => c.Id == id);
            if (conversation != null)
            {
                conversation.IsTyping = isTyping;
            }
        }

        public void MarkAsRead(string id)
        {
            var conversation = _allConversations.FirstOrDefault(c => c.Id == id);
            if (conversation != null)
            {
                conversation.UnreadCount = 0;
                conversation.MessageStatus = MessageStatus.Read;
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            
            // Subscribe to the Loaded event to ensure all child controls are loaded
            this.Loaded += (s, args) =>
            {
                // Make sure ConversationItems are connected to events
                RefreshItemEventHandlers();
            };
            
            // Also subscribe to the CollectionChanged event for Conversations
            if (Conversations != null)
            {
                Conversations.CollectionChanged += (s, args) =>
                {
                    // When items are added, make sure they're connected
                    RefreshItemEventHandlers();
                };
            }
        }

        private void RefreshItemEventHandlers()
        {
            // Find all conversation items in the visual tree and attach event handlers
            var conversationItems = this.ConversationsItemsControl.FindVisualChildren<ConversationItem>();
            foreach (var item in conversationItems)
            {
                // Remove existing handlers to prevent duplicates
                item.Selected -= ConversationItem_Selected!;
                
                // Add the handler
                item.Selected += ConversationItem_Selected!;
            }
        }

        private void FilterConversations()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                Conversations = new ObservableCollection<ConversationItemViewModel>(_allConversations);
            }
            else
            {
                var filtered = _allConversations.Where(c => 
                    c.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) || 
                    c.LastMessage.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
                Conversations = new ObservableCollection<ConversationItemViewModel>(filtered);
            }
        }

        #endregion
    }
} 