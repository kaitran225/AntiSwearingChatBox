using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AntiSwearingChatBox.App.Components
{
    /// <summary>
    /// Interaction logic for MessageInputBox.xaml
    /// </summary>
    public partial class MessageInputBox : UserControl
    {
        public MessageInputBox()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        #region Dependency Properties

        public static readonly DependencyProperty MessageTextProperty =
            DependencyProperty.Register("MessageText", typeof(string), typeof(MessageInputBox), 
                new PropertyMetadata("", OnMessageTextChanged));

        public string MessageText
        {
            get { return (string)GetValue(MessageTextProperty); }
            set { SetValue(MessageTextProperty, value); }
        }

        public static readonly DependencyProperty CanSendProperty =
            DependencyProperty.Register("CanSend", typeof(bool), typeof(MessageInputBox), 
                new PropertyMetadata(false));

        public bool CanSend
        {
            get { return (bool)GetValue(CanSendProperty); }
            set { SetValue(CanSendProperty, value); }
        }

        #endregion

        #region Events

        public event EventHandler<string>? MessageSent;
        public event EventHandler? AttachmentRequested;

        #endregion

        private static void OnMessageTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var inputBox = d as MessageInputBox;
            if (inputBox == null) return;

            // Enable send button only if message is not empty
            inputBox.CanSend = !string.IsNullOrWhiteSpace(inputBox.MessageText);
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                SendMessage();
                e.Handled = true;
            }
        }

        private void AttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            AttachmentRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SendMessage()
        {
            if (CanSend)
            {
                MessageSent?.Invoke(this, MessageText);
                MessageText = string.Empty;
                MessageTextBox.Focus();
            }
        }

        public void FocusTextBox()
        {
            MessageTextBox.Focus();
        }
    }
} 