using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AntiSwearingChatBox.WPF.Components
{
    public partial class ChatBubble : UserControl
    {
        public ChatBubble()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region Properties

        public static readonly DependencyProperty IsSentProperty =
            DependencyProperty.Register("IsSent", typeof(bool), typeof(ChatBubble), 
                new PropertyMetadata(false, OnBubbleTypeChanged));

        public static readonly DependencyProperty IsReceivedProperty =
            DependencyProperty.Register("IsReceived", typeof(bool), typeof(ChatBubble), 
                new PropertyMetadata(true, OnBubbleTypeChanged));

        public static readonly DependencyProperty AvatarProperty =
            DependencyProperty.Register("Avatar", typeof(string), typeof(ChatBubble), 
                new PropertyMetadata(""));

        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.Register("MessageText", typeof(string), typeof(ChatBubble), 
                new PropertyMetadata(""));

        public static readonly DependencyProperty TimestampProperty =
            DependencyProperty.Register("Timestamp", typeof(string), typeof(ChatBubble), 
                new PropertyMetadata(""));

        public static readonly DependencyProperty BubbleBackgroundProperty =
            DependencyProperty.Register("BubbleBackground", typeof(Brush), typeof(ChatBubble), 
                new PropertyMetadata(null));

        public static readonly DependencyProperty BubbleBorderProperty =
            DependencyProperty.Register("BubbleBorder", typeof(Brush), typeof(ChatBubble), 
                new PropertyMetadata(null));

        public static readonly DependencyProperty BubbleColumnProperty =
            DependencyProperty.Register("BubbleColumn", typeof(int), typeof(ChatBubble), 
                new PropertyMetadata(1));

        public static readonly DependencyProperty BubbleAlignmentProperty =
            DependencyProperty.Register("BubbleAlignment", typeof(HorizontalAlignment), typeof(ChatBubble), 
                new PropertyMetadata(HorizontalAlignment.Left));

        public static readonly DependencyProperty BubbleMarginProperty =
            DependencyProperty.Register("BubbleMargin", typeof(Thickness), typeof(ChatBubble), 
                new PropertyMetadata(new Thickness(8, 0, 0, 0)));

        public bool IsSent
        {
            get { return (bool)GetValue(IsSentProperty); }
            set { SetValue(IsSentProperty, value); }
        }

        public bool IsReceived
        {
            get { return (bool)GetValue(IsReceivedProperty); }
            set { SetValue(IsReceivedProperty, value); }
        }

        public string Avatar
        {
            get { return (string)GetValue(AvatarProperty); }
            set { SetValue(AvatarProperty, value); }
        }

        public string MessageText
        {
            get { return (string)GetValue(MessageTextProperty); }
            set { SetValue(MessageTextProperty, value); }
        }

        public string Timestamp
        {
            get { return (string)GetValue(TimestampProperty); }
            set { SetValue(TimestampProperty, value); }
        }

        public Brush BubbleBackground
        {
            get { return (Brush)GetValue(BubbleBackgroundProperty); }
            set { SetValue(BubbleBackgroundProperty, value); }
        }

        public Brush BubbleBorder
        {
            get { return (Brush)GetValue(BubbleBorderProperty); }
            set { SetValue(BubbleBorderProperty, value); }
        }

        public int BubbleColumn
        {
            get { return (int)GetValue(BubbleColumnProperty); }
            set { SetValue(BubbleColumnProperty, value); }
        }

        public HorizontalAlignment BubbleAlignment
        {
            get { return (HorizontalAlignment)GetValue(BubbleAlignmentProperty); }
            set { SetValue(BubbleAlignmentProperty, value); }
        }

        public Thickness BubbleMargin
        {
            get { return (Thickness)GetValue(BubbleMarginProperty); }
            set { SetValue(BubbleMarginProperty, value); }
        }

        #endregion

        private static void OnBubbleTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var bubble = d as ChatBubble;
            if (bubble == null) return;

            if (bubble.IsSent)
            {
                bubble.BubbleAlignment = HorizontalAlignment.Right;
                bubble.BubbleColumn = 1;
                bubble.BubbleMargin = new Thickness(0, 0, 8, 0);
                bubble.IsReceived = false;
            }
            else
            {
                bubble.BubbleAlignment = HorizontalAlignment.Left;
                bubble.BubbleColumn = 1;
                bubble.BubbleMargin = new Thickness(8, 0, 0, 0);
                bubble.IsReceived = true;
            }
        }
    }
} 