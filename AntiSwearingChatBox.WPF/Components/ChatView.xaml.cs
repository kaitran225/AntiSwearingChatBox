using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Input;

namespace AntiSwearingChatBox.WPF.Components
{
    /// <summary>
    /// Simple view model for contact information in the chat view
    /// </summary>
    public class ContactViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string _id = string.Empty;
        private string _name = string.Empty;
        private string _initials = string.Empty;
        
        public string Id
        {
            get => _id;
            set 
            { 
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        
        public string Name
        {
            get => _name;
            set 
            { 
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        
        public string Initials
        {
            get => _initials;
            set 
            { 
                _initials = value;
                OnPropertyChanged(nameof(Initials));
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// Interaction logic for ChatView.xaml
    /// </summary>
    public partial class ChatView : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<string>? MessageSent;
        public event EventHandler? MenuRequested;
        public event EventHandler? AttachmentRequested;
        public event EventHandler? NewConversationRequested;

        public ObservableCollection<ChatMessageViewModel> Messages { get; private set; }

        public ChatView()
        {
            InitializeComponent();
            this.DataContext = this;
            Messages = [];
            
            // Make sure the ItemsSource is set
            if (MessagesList != null)
            {
                MessagesList.ItemsSource = Messages;
            }
            
            // Add the KeyDown event handler for Enter key
            MessageTextBox.KeyDown += MessageTextBox_KeyDown;
        }

        #region Properties

        private ContactViewModel? _currentContact;
        public ContactViewModel? CurrentContact
        {
            get { return _currentContact; }
            set
            {
                _currentContact = value;
                OnPropertyChanged(nameof(CurrentContact));
                OnPropertyChanged(nameof(HasSelectedConversation));
                
                // When contact changes, update UI visibility
                UpdateUIVisibility();
            }
        }

        public bool HasSelectedConversation
        {
            get { return _currentContact != null; }
        }

        #endregion

        #region Event Handlers

        private void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Handle Enter key press to send message
            if (e.Key == Key.Enter)
            {
                // Only handle if not already handled by another handler
                if (e.Handled)
                {
                    Console.WriteLine("Enter key event already handled, skipping");
                    return;
                }
                
                // Shift+Enter creates a new line, just Enter sends the message
                if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                {
                    // Allow Shift+Enter to create a new line
                    Console.WriteLine("Shift+Enter pressed - allowing new line");
                    MessageTextBox.Text += Environment.NewLine;
                    MessageTextBox.CaretIndex = MessageTextBox.Text.Length; // Move caret to end
                    e.Handled = true; // Mark as handled
                }
                else
                {
                    // Just Enter - send the message
                    Console.WriteLine("Enter key pressed without Shift - sending message");
                    e.Handled = true; // Prevent default behavior
                    SendMessage();
                }
            }
        }

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Send button clicked - sending message");
            SendMessage();
        }

        // Track if we're currently in the process of sending a message to prevent duplicate sends
        private bool _isSending = false;

        private void SendMessage()
        {
            Console.WriteLine("SendMessage called");
            
            // Prevent duplicate sends
            if (_isSending)
            {
                Console.WriteLine("Already sending message, ignoring duplicate call");
                return;
            }
            
            try
            {
                _isSending = true;
                
                if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
                {
                    Console.WriteLine("Message text is empty, not sending");
                    return;
                }

                var textToSend = MessageTextBox.Text.TrimEnd();
                Console.WriteLine($"Sending message: '{textToSend}'");
                MessageSent?.Invoke(this, textToSend);
                
                Console.WriteLine("Clearing message text box");
                MessageTextBox.Clear();
            }
            finally
            {
                _isSending = false;
            }
        }

        private void MenuButton_Click(object sender, RoutedEventArgs e)
        {
            MenuRequested?.Invoke(this, EventArgs.Empty);
        }

        private void AttachmentButton_Click(object sender, RoutedEventArgs e)
        {
            AttachmentRequested?.Invoke(this, EventArgs.Empty);
        }

        private void NewConversationButton_Click(object sender, RoutedEventArgs e)
        {
            NewConversationRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Public Methods

        public void ClearChat()
        {
            Console.WriteLine("ClearChat called");
            Messages.Clear();
            CurrentContact = null;
            
            // Will trigger UI visibility update through CurrentContact setter
        }

        public void ShowChatView()
        {
            Console.WriteLine("ShowChatView called");
            
            // Force conversation selected state
            if (CurrentContact == null)
            {
                Console.WriteLine("WARNING: ShowChatView called but CurrentContact is null");
            }
            else
            {
                // Set HasSelectedConversation property to true
                OnPropertyChanged(nameof(HasSelectedConversation));
            }
            
            // Force UI update directly without using CurrentContact property
            Dispatcher.InvokeAsync(() => {
                try {
                    // Direct access to named controls in XAML
                    if (MessagesScroll != null)
                    {
                        MessagesScroll.Visibility = Visibility.Visible;
                        Console.WriteLine("Set MessagesScroll to Visible");
                    }
                    
                    if (MessagesList != null)
                    {
                        MessagesList.Visibility = Visibility.Visible;
                        Console.WriteLine("Set MessagesList to Visible");
                    }
                    
                    // Make sure the parent border of MessageTextBox is visible
                    if (MessageTextBox != null)
                    {
                        var parent = VisualTreeHelper.GetParent(MessageTextBox);
                        while (parent != null && !(parent is Border))
                        {
                            parent = VisualTreeHelper.GetParent(parent);
                        }
                        
                        if (parent is Border inputBorder)
                        {
                            inputBorder.Visibility = Visibility.Visible;
                            Console.WriteLine("Set message input Border to Visible");
                        }
                    }
                    else
                    {
                        Console.WriteLine("WARNING: MessageTextBox is null");
                    }
                    
                    // Force layout update after changing visibility
                    UpdateLayout();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error showing chat view: {ex.Message}");
                }
            });
        }

        private void UpdateUIVisibility()
        {
            try {
                // Update UI based on whether we have a selected conversation
                if (HasSelectedConversation)
                {
                    // Show chat interface
                    Console.WriteLine("HasSelectedConversation=true, showing chat interface");
                    
                    // Direct access to named controls in the XAML
                    if (MessagesScroll != null)
                    {
                        MessagesScroll.Visibility = Visibility.Visible;
                        Console.WriteLine("Set MessagesScroll to Visible");
                    }
                    
                    if (MessagesList != null)
                    {
                        MessagesList.Visibility = Visibility.Visible;
                        Console.WriteLine("Set MessagesList to Visible");
                    }
                    
                    // Find and show message input
                    if (MessageTextBox != null)
                    {
                        // Get parent border (Grid.Row="2" Border)
                        DependencyObject parent = MessageTextBox;
                        while (parent != null && !(parent is Border))
                        {
                            parent = VisualTreeHelper.GetParent(parent);
                        }
                        
                        if (parent is Border inputBorder)
                        {
                            inputBorder.Visibility = Visibility.Visible;
                            Console.WriteLine("Set message input border to Visible");
                        }
                    }
                    else
                    {
                        Console.WriteLine("WARNING: MessageTextBox is null");
                    }
                }
                else
                {
                    // Show empty state by letting the XAML bindings handle it
                    Console.WriteLine("HasSelectedConversation=false, showing empty state");
                    
                    // The empty state is shown automatically via bindings in XAML
                    // when HasSelectedConversation is false
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating UI visibility: {ex.Message}");
            }
        }

        public void ScrollToBottom()
        {
            Console.WriteLine("ScrollToBottom called");
            
            Dispatcher.InvokeAsync(() =>
            {
                try
                {
                    // Direct access to the named ScrollViewer in XAML
                    if (MessagesScroll != null)
                    {
                        Console.WriteLine("Scrolling MessagesScroll to bottom");
                        MessagesScroll.ScrollToBottom();
                    }
                    else
                    {
                        Console.WriteLine("ERROR: MessagesScroll is null");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in ScrollToBottom: {ex.Message}");
                }
            });
        }

        #endregion

        #region Helpers

        // Helper methods for finding elements in the visual tree
        private T? FindVisualChild<T>(DependencyObject? parent) where T : DependencyObject
        {
            if (parent == null)
                return null;
                
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T typedChild)
                    return typedChild;
                    
                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            
            return null;
        }
        
        private FrameworkElement? FindVisualChildByName(string name)
        {
            return FindVisualChildByName(this, name);
        }
        
        private FrameworkElement? FindVisualChildByName(DependencyObject? parent, string name)
        {
            if (parent == null)
                return null;
                
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is FrameworkElement element && element.Name == name)
                    return element;
                    
                var result = FindVisualChildByName(child, name);
                if (result != null)
                    return result;
            }
            
            return null;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 