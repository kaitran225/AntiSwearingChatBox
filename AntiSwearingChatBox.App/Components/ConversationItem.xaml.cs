using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AntiSwearingChatBox.App.Components
{
    /// <summary>
    /// Interaction logic for ConversationItem.xaml
    /// </summary>
    public partial class ConversationItem : UserControl
    {
        public ConversationItem()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region Dependency Properties

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(ConversationItem), 
                new PropertyMetadata(""));

        public static readonly DependencyProperty LastMessageProperty =
            DependencyProperty.Register("LastMessage", typeof(string), typeof(ConversationItem), 
                new PropertyMetadata(""));

        public static readonly DependencyProperty TimestampProperty =
            DependencyProperty.Register("Timestamp", typeof(string), typeof(ConversationItem), 
                new PropertyMetadata(""));

        public static readonly DependencyProperty AvatarTextProperty =
            DependencyProperty.Register("AvatarText", typeof(string), typeof(ConversationItem), 
                new PropertyMetadata(""));

        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ConversationItem), 
                new PropertyMetadata(false, OnActiveChanged));

        public static readonly DependencyProperty HasUnreadProperty =
            DependencyProperty.Register("HasUnread", typeof(bool), typeof(ConversationItem), 
                new PropertyMetadata(false));

        public static readonly DependencyProperty UnreadCountProperty =
            DependencyProperty.Register("UnreadCount", typeof(string), typeof(ConversationItem), 
                new PropertyMetadata("0"));

        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(ConversationItem), 
                new PropertyMetadata(Application.Current.Resources["SecondaryBackgroundBrush"]));

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(ConversationItem), 
                new PropertyMetadata(null));

        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(ConversationItem), 
                new PropertyMetadata(new Thickness(0)));

        public static readonly DependencyProperty AvatarForegroundProperty =
            DependencyProperty.Register("AvatarForeground", typeof(Brush), typeof(ConversationItem), 
                new PropertyMetadata(Application.Current.Resources["SecondaryTextBrush"]));

        public static readonly DependencyProperty TimestampForegroundProperty =
            DependencyProperty.Register("TimestampForeground", typeof(Brush), typeof(ConversationItem), 
                new PropertyMetadata(Application.Current.Resources["TertiaryTextBrush"]));

        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public string LastMessage
        {
            get { return (string)GetValue(LastMessageProperty); }
            set { SetValue(LastMessageProperty, value); }
        }

        public string Timestamp
        {
            get { return (string)GetValue(TimestampProperty); }
            set { SetValue(TimestampProperty, value); }
        }

        public string AvatarText
        {
            get { return (string)GetValue(AvatarTextProperty); }
            set { SetValue(AvatarTextProperty, value); }
        }

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public bool HasUnread
        {
            get { return (bool)GetValue(HasUnreadProperty); }
            set { SetValue(HasUnreadProperty, value); }
        }

        public string UnreadCount
        {
            get { return (string)GetValue(UnreadCountProperty); }
            set { SetValue(UnreadCountProperty, value); }
        }

        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        public Brush AvatarForeground
        {
            get { return (Brush)GetValue(AvatarForegroundProperty); }
            set { SetValue(AvatarForegroundProperty, value); }
        }

        public Brush TimestampForeground
        {
            get { return (Brush)GetValue(TimestampForegroundProperty); }
            set { SetValue(TimestampForegroundProperty, value); }
        }

        #endregion

        private static void OnActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var item = d as ConversationItem;
            if (item == null) return;

            if (item.IsActive)
            {
                item.BorderBrush = Application.Current.Resources["PrimaryGreenBrush"] as Brush;
                item.BorderThickness = new Thickness(1);
                item.AvatarForeground = Application.Current.Resources["PrimaryGreenBrush"] as Brush;
                item.TimestampForeground = Application.Current.Resources["PrimaryGreenBrush"] as Brush;
            }
            else
            {
                item.BorderBrush = null;
                item.BorderThickness = new Thickness(0);
                item.AvatarForeground = Application.Current.Resources["SecondaryTextBrush"] as Brush;
                item.TimestampForeground = Application.Current.Resources["TertiaryTextBrush"] as Brush;
            }
        }
    }
} 