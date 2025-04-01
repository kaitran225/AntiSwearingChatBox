using System.ComponentModel;
using System.Windows.Media;

namespace AntiSwearingChatBox.WPF.Components
{
    public class ChatMessageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _text = string.Empty;
        private bool _isSent;
        private string _timestamp = string.Empty;
        private string _avatar = string.Empty;
        private SolidColorBrush _background = new SolidColorBrush(Colors.White);
        private SolidColorBrush _borderBrush = new SolidColorBrush(Colors.LightGray);

        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 