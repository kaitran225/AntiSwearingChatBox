using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace AntiSwearingChatBox.WPF.Components
{
    /// <summary>
    /// ViewModel for Contact information in the chat view
    /// </summary>
    public class ContactInfoViewModel : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _name = string.Empty;
        private string _initials = string.Empty;
        private string _status = string.Empty;
        private string _lastMessage = string.Empty;
        private string _lastMessageTime = string.Empty;
        private bool _isOnline = false;
        private bool _isActive = false;
        
        public string Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    Initials = !string.IsNullOrEmpty(value) && value.Length > 0
                        ? value.Substring(0, 1).ToUpper()
                        : "?";
                    OnPropertyChanged();
                }
            }
        }
        
        public string Initials
        {
            get => _initials;
            set
            {
                if (_initials != value)
                {
                    _initials = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                if (_status != value)
                {
                    _status = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LastMessage
        {
            get => _lastMessage;
            set
            {
                if (_lastMessage != value)
                {
                    _lastMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string LastMessageTime
        {
            get => _lastMessageTime;
            set
            {
                if (_lastMessageTime != value)
                {
                    _lastMessageTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsOnline
        {
            get => _isOnline;
            set
            {
                if (_isOnline != value)
                {
                    _isOnline = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 