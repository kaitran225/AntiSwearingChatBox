using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AntiSwearingChatBox.WPF.Components
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

        public string Id
        {
            get => _id!;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        public string Title
        {
            get => _title!;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
                OnPropertyChanged(nameof(Avatar));
            }
        }

        public string LastMessage
        {
            get => _lastMessage!;
            set
            {
                _lastMessage = value;
                OnPropertyChanged(nameof(LastMessage));
            }
        }

        public string LastMessageTime
        {
            get => _lastMessageTime!;
            set
            {
                _lastMessageTime = value;
                OnPropertyChanged(nameof(LastMessageTime));
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set
            {
                _unreadCount = value;
                OnPropertyChanged(nameof(UnreadCount));
                OnPropertyChanged(nameof(HasUnread));
            }
        }

        public bool HasUnread => UnreadCount > 0;

        public string Avatar
        {
            get
            {
                if (string.IsNullOrEmpty(Title))
                    return "?";

                return Title.Substring(0, 1).ToUpper();
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 