using System;
using System.Windows;
using System.Windows.Controls;

namespace AntiSwearingChatBox.WPF.Components
{
    /// <summary>
    /// Interaction logic for ChatHeader.xaml
    /// </summary>
    public partial class ChatHeader : UserControl
    {
        public ChatHeader()
        {
            InitializeComponent();
            this.DataContext = this!;
        }

        #region Dependency Properties

        public static readonly DependencyProperty DisplayNameProperty =
            DependencyProperty.Register("DisplayName", typeof(string), typeof(ChatHeader), 
                new PropertyMetadata(""));

        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(ChatHeader), 
                new PropertyMetadata(""));

        public static readonly DependencyProperty AvatarTextProperty =
            DependencyProperty.Register("AvatarText", typeof(string), typeof(ChatHeader), 
                new PropertyMetadata(""));

        public static readonly DependencyProperty ShowActionsProperty =
            DependencyProperty.Register("ShowActions", typeof(bool), typeof(ChatHeader), 
                new PropertyMetadata(true));

        public string DisplayName
        {
            get { return (string)GetValue(DisplayNameProperty); }
            set { SetValue(DisplayNameProperty, value); }
        }

        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }

        public string AvatarText
        {
            get { return (string)GetValue(AvatarTextProperty); }
            set { SetValue(AvatarTextProperty, value); }
        }

        public bool ShowActions
        {
            get { return (bool)GetValue(ShowActionsProperty); }
            set { SetValue(ShowActionsProperty, value); }
        }

        #endregion

        #region Events

        public event EventHandler? MenuRequested;

        #endregion

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            MenuRequested?.Invoke(this, EventArgs.Empty);
        }
    }
} 