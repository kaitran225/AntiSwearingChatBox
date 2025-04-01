using System.ComponentModel;
using System.Windows.Media;

namespace AntiSwearingChatBox.App.Components
{
    public class ChatMessageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string? _text;
        private string? _timestamp;
        private string? _avatar;
        private bool _isSent;
        private Brush? _background;
        private Brush? _borderBrush;

        public string Text
        {
            get => _text!;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        public string Timestamp
        {
            get => _timestamp!;
            set
            {
                _timestamp = value;
                OnPropertyChanged(nameof(Timestamp));
            }
        }

        public string Avatar
        {
            get => _avatar!;
            set
            {
                _avatar = value;
                OnPropertyChanged(nameof(Avatar));
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

        public Brush Background
        {
            get => _background!;
            set
            {
                _background = value;
                OnPropertyChanged(nameof(Background));
            }
        }

        public Brush BorderBrush
        {
            get => _borderBrush!;
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