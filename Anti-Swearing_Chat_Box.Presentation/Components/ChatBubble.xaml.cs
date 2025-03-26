using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Anti_Swearing_Chat_Box.Presentation.Controls
{
    public partial class ChatBubble : UserControl
    {
        public static readonly DependencyProperty UsernameProperty =
            DependencyProperty.Register("Username", typeof(string), typeof(ChatBubble), new PropertyMetadata(string.Empty, OnUsernameChanged));

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(string), typeof(ChatBubble), new PropertyMetadata(string.Empty, OnMessageChanged));

        public static readonly DependencyProperty TimeProperty =
            DependencyProperty.Register("Time", typeof(string), typeof(ChatBubble), new PropertyMetadata(string.Empty, OnTimeChanged));

        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register("Status", typeof(string), typeof(ChatBubble), new PropertyMetadata(string.Empty, OnStatusChanged));

        public static readonly DependencyProperty IsOwnMessageProperty =
            DependencyProperty.Register("IsOwnMessage", typeof(bool), typeof(ChatBubble), new PropertyMetadata(false, OnIsOwnMessageChanged));

        public string Username
        {
            get { return (string)GetValue(UsernameProperty); }
            set { SetValue(UsernameProperty, value); }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public string Time
        {
            get { return (string)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public string Status
        {
            get { return (string)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public bool IsOwnMessage
        {
            get { return (bool)GetValue(IsOwnMessageProperty); }
            set { SetValue(IsOwnMessageProperty, value); }
        }

        public ChatBubble()
        {
            InitializeComponent();
            Loaded += ChatBubble_Loaded;
        }

        private void ChatBubble_Loaded(object sender, RoutedEventArgs e)
        {
            var storyboard = (Storyboard)FindResource("FadeInAnimation");
            storyboard.Begin(this);
            UpdateMessageStyle();
        }

        private void UpdateMessageStyle()
        {
            if (IsOwnMessage)
            {
                // Own message styling
                MessageBorder.Background = FindResource("MatchaGreenBrush") as SolidColorBrush;
                MainGrid.HorizontalAlignment = HorizontalAlignment.Right;
                UsernameText.HorizontalAlignment = HorizontalAlignment.Right;
                StatusIcon.Visibility = Visibility.Visible;
            }
            else
            {
                // Other person's message styling
                MessageBorder.Background = FindResource("ReceivedMessageBrush") as SolidColorBrush;
                MainGrid.HorizontalAlignment = HorizontalAlignment.Left;
                UsernameText.HorizontalAlignment = HorizontalAlignment.Left;
                StatusIcon.Visibility = Visibility.Collapsed;
            }
        }

        private static void OnUsernameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ChatBubble;
            if (control != null)
            {
                control.UsernameText.Text = e.NewValue as string;
            }
        }

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ChatBubble;
            if (control != null)
            {
                control.MessageText.Text = e.NewValue as string;
            }
        }

        private static void OnTimeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ChatBubble;
            if (control != null)
            {
                control.TimeText.Text = e.NewValue as string;
            }
        }

        private static void OnStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ChatBubble;
            if (control != null && control.StatusIcon != null)
            {
                string status = e.NewValue as string;
                
                if (status == "Sent")
                {
                    control.StatusIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Check;
                    control.StatusIcon.Foreground = new SolidColorBrush(Colors.White);
                }
                else if (status == "Delivered")
                {
                    control.StatusIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckAll;
                    control.StatusIcon.Foreground = new SolidColorBrush(Colors.White);
                }
                else if (status == "Read")
                {
                    control.StatusIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.CheckAll;
                    control.StatusIcon.Foreground = Application.Current.Resources["AccentBrightGreenBrush"] as Brush;
                }
            }
        }

        private static void OnIsOwnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as ChatBubble;
            if (control != null)
            {
                control.UpdateMessageStyle();
            }
        }
    }
} 