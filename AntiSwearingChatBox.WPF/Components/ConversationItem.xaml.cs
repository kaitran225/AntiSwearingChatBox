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
                new PropertyMetadata(false));

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

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            IsSelected = true;
            string? e1 = Tag as string;
            Selected?.Invoke(this, e1!);
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
    }
} 