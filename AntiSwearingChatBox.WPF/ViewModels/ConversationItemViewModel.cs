using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using AntiSwearingChatBox.WPF.Components; // Added for MessageStatus enum

namespace AntiSwearingChatBox.WPF.ViewModels // Changed namespace to match ViewModels folder
{
    /// <summary>
    /// ViewModel for conversation items in the conversation list
    /// </summary>
    public class ConversationItemViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string? _id;
        private string? _title;
        private string? _lastMessage;
        private string? _lastMessageTime;
        private bool _isSelected;
        private int _unreadCount;
        private bool _isOnline;
        private string? _lastSeen;
        private bool _showLastSeen;
        private bool _isTyping;
        private MessageStatus _messageStatus;
        private int? _swearingScore;
        private bool _isClosed;
        private string? _avatarText;
        private DateTime _sortTimestamp = DateTime.Now; // Default to current time

        public string Id
        {
            get => _id!;
            set { SetField(ref _id, value); }
        }

        public string Title
        {
            get => _title!;
            set 
            { 
                SetField(ref _title, value);
                OnPropertyChanged(nameof(Avatar)); // Update Avatar when Title changes
            }
        }

        public string LastMessage
        {
            get => _lastMessage!;
            set { SetField(ref _lastMessage, value); }
        }

        public string LastMessageTime
        {
            get => _lastMessageTime!;
            set { SetField(ref _lastMessageTime, value); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { SetField(ref _isSelected, value); }
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set 
            { 
                SetField(ref _unreadCount, value);
                OnPropertyChanged(nameof(HasUnread)); // Update HasUnread when UnreadCount changes
            }
        }

        // Calculated property based on Title
        public string Avatar => !string.IsNullOrEmpty(Title) ? Title[0].ToString().ToUpper() : "?";

        // AvatarText property that can be set independently
        public string AvatarText
        {
            get => _avatarText ?? Avatar;
            set { SetField(ref _avatarText, value); }
        }

        // Calculated property based on UnreadCount
        public bool HasUnread => UnreadCount > 0;

        // New properties added to fix build errors
        public bool IsOnline
        {
            get => _isOnline;
            set { SetField(ref _isOnline, value); }
        }

        public string? LastSeen
        {
            get => _lastSeen;
            set { SetField(ref _lastSeen, value); }
        }

        public bool ShowLastSeen
        {
            get => _showLastSeen;
            set { SetField(ref _showLastSeen, value); }
        }

        public bool IsTyping
        {
            get => _isTyping;
            set { SetField(ref _isTyping, value); }
        }

        public MessageStatus MessageStatus
        {
            get => _messageStatus;
            set { SetField(ref _messageStatus, value); }
        }

        public int? SwearingScore
        {
            get => _swearingScore;
            set { SetField(ref _swearingScore, value); }
        }

        public bool IsClosed
        {
            get => _isClosed;
            set { SetField(ref _isClosed, value); }
        }
        
        // Timestamp used for sorting conversations (not visible in UI)
        public DateTime SortTimestamp
        {
            get => _sortTimestamp;
            set { SetField(ref _sortTimestamp, value); }
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
} 