using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AntiSwearingChatBox.App.Components
{
    /// <summary>
    /// Interaction logic for ConversationList.xaml
    /// </summary>
    public partial class ConversationList : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ConversationList()
        {
            InitializeComponent();
            this.DataContext = this;
            Conversations = new ObservableCollection<ConversationItemViewModel>();
            Groups = new ObservableCollection<ConversationItemViewModel>();
        }

        #region Properties

        private ObservableCollection<ConversationItemViewModel> _conversations;
        public ObservableCollection<ConversationItemViewModel> Conversations
        {
            get { return _conversations; }
            set
            {
                _conversations = value;
                OnPropertyChanged(nameof(Conversations));
            }
        }

        private ObservableCollection<ConversationItemViewModel> _groups;
        public ObservableCollection<ConversationItemViewModel> Groups
        {
            get { return _groups; }
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

        public bool HasGroups => Groups?.Count > 0;

        #endregion

        #region Events

        public event EventHandler<string> ConversationSelected;
        public event EventHandler NewChatRequested;
        public event EventHandler AddConversationRequested;
        public event EventHandler AdminDashboardRequested;

        #endregion

        #region Event Handlers

        private void ConversationItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ConversationItem item && item.Tag is string id)
            {
                // Update active states
                foreach (var conv in Conversations)
                {
                    conv.IsSelected = conv.Id == id;
                }

                foreach (var group in Groups)
                {
                    group.IsSelected = group.Id == id;
                }

                // Reset unread count for selected conversation
                var selectedConv = Conversations.FirstOrDefault(c => c.Id == id);
                if (selectedConv != null)
                {
                    selectedConv.UnreadCount = 0;
                }

                var selectedGroup = Groups.FirstOrDefault(g => g.Id == id);
                if (selectedGroup != null)
                {
                    selectedGroup.UnreadCount = 0;
                }

                ConversationSelected?.Invoke(this, id);
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Filter conversations here if needed
            string searchText = SearchBox.Text.ToLower();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // Show all conversations
                foreach (var contact in Conversations)
                {
                    // Show all items by setting their visibility property if you have one
                    // contact.IsVisible = true;
                }
            }
            else
            {
                // Filter conversations
                foreach (var contact in Conversations)
                {
                    if (contact.Title.ToLower().Contains(searchText) || 
                        contact.LastMessage.ToLower().Contains(searchText))
                    {
                        // contact.IsVisible = true;
                    }
                    else
                    {
                        // contact.IsVisible = false;
                    }
                }
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

        #endregion

        #region Methods

        public void AddConversation(ConversationItemViewModel conversation)
        {
            Conversations.Add(conversation);
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
            
            Conversations.Add(conversation);
        }

        public void AddGroup(ConversationItemViewModel group)
        {
            Groups.Add(group);
            OnPropertyChanged(nameof(HasGroups));
        }

        public void UpdateConversation(string id, string lastMessage, string timestamp, bool increaseUnread = false)
        {
            var conversation = Conversations.FirstOrDefault(c => c.Id == id);
            if (conversation != null)
            {
                conversation.LastMessage = lastMessage;
                conversation.LastMessageTime = timestamp;

                if (increaseUnread && !conversation.IsSelected)
                {
                    conversation.UnreadCount++;
                }
            }

            var group = Groups.FirstOrDefault(g => g.Id == id);
            if (group != null)
            {
                group.LastMessage = lastMessage;
                group.LastMessageTime = timestamp;

                if (increaseUnread && !group.IsSelected)
                {
                    group.UnreadCount++;
                }
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 