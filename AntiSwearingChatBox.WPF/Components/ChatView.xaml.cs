using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;

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

        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            MessageSent?.Invoke(this, MessageTextBox.Text);
            MessageTextBox.Clear();
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

        #region Methods

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        // Helper method to find all elements of a specific type in the visual tree
        private IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T t)
                    yield return t;
                
                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
        }

        #endregion

        private T FindVisualChild<T>() where T : DependencyObject
        {
            foreach (object child in LogicalTreeHelper.GetChildren(this))
            {
                if (child is T typedChild)
                    return typedChild;
                
                if (child is DependencyObject depChild)
                {
                    T childOfChild = FindVisualChildHelper<T>(depChild);
                    if (childOfChild != null)
                        return childOfChild;
                }
            }
            return null;
        }

        private T FindVisualChildHelper<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is T typedChild)
                    return typedChild;
                
                T childOfChild = FindVisualChildHelper<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        // Helper method to find a named element in the visual tree
        private DependencyObject FindVisualChildByName(string name)
        {
            foreach (object child in LogicalTreeHelper.GetChildren(this))
            {
                if (child is FrameworkElement element && element.Name == name)
                    return element;
                
                if (child is DependencyObject depChild)
                {
                    DependencyObject childResult = FindVisualChildByNameHelper(depChild, name);
                    if (childResult != null)
                        return childResult;
                }
            }
            return null;
        }

        private DependencyObject FindVisualChildByNameHelper(DependencyObject parent, string name)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                
                if (child is FrameworkElement element && element.Name == name)
                    return element;
                
                DependencyObject childOfChild = FindVisualChildByNameHelper(child, name);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
} 