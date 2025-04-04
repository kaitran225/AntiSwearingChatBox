using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AntiSwearingChatBox.WPF.Components
{
    /// <summary>
    /// Interaction logic for ConversationItem.xaml
    /// </summary>
    public partial class ConversationItem : UserControl
    {
        public event EventHandler<string>? Selected;

        public ConversationItem()
        {
            InitializeComponent();
            this.DataContext = this;
            UpdateVisualState();
        }

        #region Properties

        // DisplayName property (alias for Title)
        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(ConversationItem),
                new PropertyMetadata(string.Empty));

        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        // Title property
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(ConversationItem),
                new PropertyMetadata(string.Empty));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Last message property
        public static readonly DependencyProperty LastMessageProperty =
            DependencyProperty.Register("LastMessage", typeof(string), typeof(ConversationItem),
                new PropertyMetadata(string.Empty));

        public string LastMessage
        {
            get { return (string)GetValue(LastMessageProperty); }
            set { SetValue(LastMessageProperty, value); }
        }

        // LastMessageTime property (alias for Timestamp)
        public static readonly DependencyProperty LastMessageTimeProperty =
            DependencyProperty.Register("LastMessageTime", typeof(string), typeof(ConversationItem),
                new PropertyMetadata(string.Empty));

        public string LastMessageTime
        {
            get { return (string)GetValue(LastMessageTimeProperty); }
            set { SetValue(LastMessageTimeProperty, value); }
        }
        
        // Timestamp property
        public static readonly DependencyProperty TimestampProperty =
            DependencyProperty.Register("Timestamp", typeof(string), typeof(ConversationItem),
                new PropertyMetadata(string.Empty));

        public string Timestamp
        {
            get { return (string)GetValue(TimestampProperty); }
            set { SetValue(TimestampProperty, value); }
        }

        // AvatarText property
        public static readonly DependencyProperty AvatarTextProperty =
            DependencyProperty.Register("AvatarText", typeof(string), typeof(ConversationItem),
                new PropertyMetadata(string.Empty));

        public string AvatarText
        {
            get { return (string)GetValue(AvatarTextProperty); }
            set { SetValue(AvatarTextProperty, value); }
        }

        // Background property
        public static new readonly DependencyProperty? BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(ConversationItem),
                new PropertyMetadata(null));

        public new Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        // Border brush property
        public static new readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(ConversationItem),
                new PropertyMetadata(null));

        public new Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        // Border thickness property
        public static new readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(ConversationItem),
                new PropertyMetadata(new Thickness(0)));

        public new Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        // IsSelected property
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(ConversationItem),
                new PropertyMetadata(false, OnIsSelectedChanged));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }
        
        // IsActive property (alias for IsSelected)
        public static readonly DependencyProperty IsActiveProperty =
            DependencyProperty.Register("IsActive", typeof(bool), typeof(ConversationItem),
                new PropertyMetadata(false, OnIsActiveChanged));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        // HasUnread property
        public static readonly DependencyProperty HasUnreadProperty =
            DependencyProperty.Register("HasUnread", typeof(bool), typeof(ConversationItem),
                new PropertyMetadata(false));

        public bool HasUnread
        {
            get { return (bool)GetValue(HasUnreadProperty); }
            set { SetValue(HasUnreadProperty, value); }
        }

        // UnreadCount property
        public static readonly DependencyProperty UnreadCountProperty =
            DependencyProperty.Register("UnreadCount", typeof(int), typeof(ConversationItem),
                new PropertyMetadata(0));

        public int UnreadCount
        {
            get { return (int)GetValue(UnreadCountProperty); }
            set { SetValue(UnreadCountProperty, value); }
        }

        // Online Status Properties
        public static readonly DependencyProperty IsOnlineProperty =
            DependencyProperty.Register(nameof(IsOnline), typeof(bool), typeof(ConversationItem),
                new PropertyMetadata(false));

        public bool IsOnline
        {
            get { return (bool)GetValue(IsOnlineProperty); }
            set { SetValue(IsOnlineProperty, value); }
        }

        public static readonly DependencyProperty LastSeenProperty =
            DependencyProperty.Register(nameof(LastSeen), typeof(string), typeof(ConversationItem),
                new PropertyMetadata(string.Empty));

        public string LastSeen
        {
            get { return (string)GetValue(LastSeenProperty); }
            set { SetValue(LastSeenProperty, value); }
        }

        public static readonly DependencyProperty ShowLastSeenProperty =
            DependencyProperty.Register(nameof(ShowLastSeen), typeof(bool), typeof(ConversationItem),
                new PropertyMetadata(false));

        public bool ShowLastSeen
        {
            get { return (bool)GetValue(ShowLastSeenProperty); }
            set { SetValue(ShowLastSeenProperty, value); }
        }

        // Typing Indicator Properties
        public static readonly DependencyProperty IsTypingProperty =
            DependencyProperty.Register(nameof(IsTyping), typeof(bool), typeof(ConversationItem),
                new PropertyMetadata(false));

        public bool IsTyping
        {
            get { return (bool)GetValue(IsTypingProperty); }
            set { SetValue(IsTypingProperty, value); }
        }

        // Message Status Properties
        public static readonly DependencyProperty MessageStatusProperty =
            DependencyProperty.Register(nameof(MessageStatus), typeof(MessageStatus), typeof(ConversationItem),
                new PropertyMetadata(MessageStatus.Sent));

        public MessageStatus MessageStatus
        {
            get { return (MessageStatus)GetValue(MessageStatusProperty); }
            set { SetValue(MessageStatusProperty, value); }
        }

        #endregion

        #region Event Handlers

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConversationItem item)
            {
                if ((bool)e.NewValue)
                {
                    // Selected appearance
                    item.Background = (Application.Current.Resources["SecondaryBackgroundBrush"] as SolidColorBrush)!;
                    item.BorderBrush = (Application.Current.Resources["PrimaryGreenBrush"] as SolidColorBrush)!;
                    item.BorderThickness = new Thickness(0, 0, 5, 0);
                }
                else
                {
                    // Normal appearance
                    item.Background = null!;
                    item.BorderBrush = (Application.Current.Resources["BorderBrush"] as SolidColorBrush)!;
                    item.BorderThickness = new Thickness(0);
                }
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine($"ConversationItem clicked with Tag: {this.Tag}");
            if (this.Tag is string id)
            {
                Console.WriteLine($"Firing Selected event with id: {id}");
                Selected?.Invoke(this, id);
                
                // Mark as selected in UI
                IsSelected = true;
                IsActive = true;
                
                // Mark as handled to prevent event bubbling
                e.Handled = true;
            }
            else
            {
                Console.WriteLine($"ConversationItem clicked but Tag is not a string: {this.Tag}");
            }
        }

        private static void OnIsActiveChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ConversationItem item)
            {
                item.UpdateVisualState();
            }
        }

        private void UpdateVisualState()
        {
            if (IsActive)
            {
                Background = (Brush)Application.Current.Resources["TertiaryBackgroundBrush"];
                BorderBrush = (Brush)Application.Current.Resources["PrimaryGreenBrush"];
                BorderThickness = new Thickness(1);
            }
            else
            {
                Background = (Brush)Application.Current.Resources["SecondaryBackgroundBrush"];
                BorderBrush = (Brush)Application.Current.Resources["BorderBrush"];
                BorderThickness = new Thickness(0);
            }
        }

        #endregion
        
        // Define UnreadBadge as a property since it's missing from the XAML
        private TextBlock? _unreadBadge;
        public TextBlock UnreadBadge
        {
            get
            {
                _unreadBadge ??= new TextBlock();
                return _unreadBadge;
            }
        }

        #region Public Methods

        public void UpdateMessageStatus(MessageStatus status)
        {
            MessageStatus = status;
        }

        public void UpdateOnlineStatus(bool isOnline, string? lastSeen = null)
        {
            IsOnline = isOnline;
            if (!isOnline && !string.IsNullOrEmpty(lastSeen))
            {
                LastSeen = lastSeen;
                ShowLastSeen = true;
            }
            else
            {
                ShowLastSeen = false;
            }
        }

        public void SetTyping(bool isTyping)
        {
            IsTyping = isTyping;
        }

        #endregion
    }

    public enum MessageStatus
    {
        Sent,
        Delivered,
        Read,
        Failed
    }
} 