using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Anti_Swearing_Chat_Box.Presentation.Controls
{
    public partial class ContactItem : UserControl
    {
        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(ContactItem), new PropertyMetadata(string.Empty, OnDisplayNameChanged));

        public static readonly DependencyProperty InitialsProperty =
            DependencyProperty.Register("Initials", typeof(string), typeof(ContactItem), new PropertyMetadata(string.Empty, OnInitialsChanged));

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(ContactItem), new PropertyMetadata(string.Empty, OnStatusTextChanged));

        public static readonly DependencyProperty IsOnlineProperty =
            DependencyProperty.Register("IsOnline", typeof(bool), typeof(ContactItem), new PropertyMetadata(false, OnIsOnlineChanged));

        public static readonly DependencyProperty AvatarColorProperty =
            DependencyProperty.Register("AvatarColor", typeof(Brush), typeof(ContactItem), new PropertyMetadata(null, OnAvatarColorChanged));

        public static readonly DependencyProperty ActionContentSourceProperty =
            DependencyProperty.Register("ActionContentSource", typeof(object), typeof(ContactItem), new PropertyMetadata(null, OnActionContentSourceChanged));

        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public string Initials
        {
            get { return (string)GetValue(InitialsProperty); }
            set { SetValue(InitialsProperty, value); }
        }

        public string Status
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        public bool IsOnline
        {
            get { return (bool)GetValue(IsOnlineProperty); }
            set { SetValue(IsOnlineProperty, value); }
        }

        public Brush AvatarColor
        {
            get { return (Brush)GetValue(AvatarColorProperty); }
            set { SetValue(AvatarColorProperty, value); }
        }

        public object ActionContentSource
        {
            get { return GetValue(ActionContentSourceProperty); }
            set { SetValue(ActionContentSourceProperty, value); }
        }

        public ContactItem()
        {
            InitializeComponent();
        }

        private static void OnDisplayNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ContactItem;
            if (control != null)
            {
                control.NameText.Text = e.NewValue as string;
            }
        }

        private static void OnInitialsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ContactItem;
            if (control != null)
            {
                control.InitialsText.Text = e.NewValue as string;
            }
        }

        private static void OnStatusTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ContactItem;
            if (control != null)
            {
                control.StatusText.Text = e.NewValue as string;
            }
        }

        private static void OnIsOnlineChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ContactItem;
            if (control != null)
            {
                bool isOnline = (bool)e.NewValue;
                if (isOnline)
                {
                    control.StatusText.Text = "Online";
                    control.StatusText.Foreground = Application.Current.Resources["AccentBrightGreenBrush"] as Brush;
                }
                else
                {
                    control.StatusText.Text = "Offline";
                    control.StatusText.Foreground = Application.Current.Resources["NeutralGrayBrush"] as Brush;
                }
            }
        }

        private static void OnAvatarColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ContactItem;
            if (control != null && e.NewValue != null)
            {
                control.AvatarBorder.Background = e.NewValue as Brush;
            }
        }

        private static void OnActionContentSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ContactItem;
            if (control != null)
            {
                control.ActionContentControl.Content = e.NewValue;
            }
        }
    }
} 