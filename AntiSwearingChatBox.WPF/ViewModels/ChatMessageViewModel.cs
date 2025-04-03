using System.ComponentModel;
using System.Windows.Media;

namespace AntiSwearingChatBox.WPF.Components
{
    public class ChatMessageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _text = string.Empty;
        private string _originalText = string.Empty;
        private bool _isSent;
        private string _timestamp = string.Empty;
        private string _avatar = string.Empty;
        private SolidColorBrush _background = new SolidColorBrush(Colors.White);
        private SolidColorBrush _borderBrush = new SolidColorBrush(Colors.LightGray);
        private bool _containsProfanity;
        private bool _isUncensored;
        private bool _sendFailed;

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public string OriginalText
        {
            get => _originalText;
            set
            {
                _originalText = value;
                OnPropertyChanged(nameof(OriginalText));
            }
        }

        public bool IsSent
        {
            get => _isSent;
            set
            {
                _isSent = value;
                OnPropertyChanged(nameof(IsSent));
            }
        }

        public string Timestamp
        {
            get => _timestamp;
            set
            {
                _timestamp = value;
                OnPropertyChanged(nameof(Timestamp));
            }
        }

        public string Avatar
        {
            get => _avatar;
            set
            {
                _avatar = value;
                OnPropertyChanged(nameof(Avatar));
            }
        }

        public SolidColorBrush Background
        {
            get => _background;
            set
            {
                _background = value;
                OnPropertyChanged(nameof(Background));
            }
        }

        public SolidColorBrush BorderBrush
        {
            get => _borderBrush;
            set
            {
                _borderBrush = value;
                OnPropertyChanged(nameof(BorderBrush));
            }
        }
        
        public bool ContainsProfanity
        {
            get => _containsProfanity;
            set
            {
                _containsProfanity = value;
                OnPropertyChanged(nameof(ContainsProfanity));
                OnPropertyChanged(nameof(HasWarningIndicator));
            }
        }
        
        public bool IsUncensored
        {
            get => _isUncensored;
            set
            {
                _isUncensored = value;
                OnPropertyChanged(nameof(IsUncensored));
                OnPropertyChanged(nameof(DisplayText));
            }
        }
        
        public bool SendFailed
        {
            get => _sendFailed;
            set
            {
                _sendFailed = value;
                OnPropertyChanged(nameof(SendFailed));
                OnPropertyChanged(nameof(HasErrorIndicator));
            }
        }
        
        public string DisplayText => IsUncensored ? OriginalText : Text;
        
        public bool HasWarningIndicator => ContainsProfanity;
        
        public bool HasErrorIndicator => SendFailed;
        
        public void ToggleCensorship()
        {
            IsUncensored = !IsUncensored;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 