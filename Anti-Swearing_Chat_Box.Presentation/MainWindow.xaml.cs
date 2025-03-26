using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Anti_Swearing_Chat_Box.AI;
using Microsoft.Extensions.Options;

namespace Anti_Swearing_Chat_Box.Presentation;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // Global variables for storing state
    private ObservableCollection<ContactModel> Contacts { get; set; } = new ObservableCollection<ContactModel>();
    private ObservableCollection<ChatThreadModel> ChatThreads { get; set; } = new ObservableCollection<ChatThreadModel>();
    private ContactModel CurrentContact { get; set; }
    private ChatThreadModel CurrentChatThread { get; set; }
    private bool IsInChatMode { get; set; } = true;
    
    // AI Service for message moderation
    private readonly GeminiService _geminiService;
    
    public MainWindow()
    {
        InitializeComponent();
        
        // Initialize Gemini AI service
        var settings = new GeminiSettings();
        _geminiService = new GeminiService(Options.Create(settings));
        
        InitializeMockData();
    }
    
    /// <summary>
    /// Initialize mock data for testing
    /// </summary>
    private void InitializeMockData()
    {
        // Add mock contacts
        Contacts.Add(new ContactModel
        {
            Id = Guid.NewGuid(),
            DisplayName = "John Doe",
            Initials = "JD",
            IsOnline = true,
            LastSeen = DateTime.Now
        });
        
        Contacts.Add(new ContactModel
        {
            Id = Guid.NewGuid(),
            DisplayName = "Jane Smith",
            Initials = "JS",
            IsOnline = false,
            LastSeen = DateTime.Now.AddHours(-2)
        });
        
        Contacts.Add(new ContactModel
        {
            Id = Guid.NewGuid(),
            DisplayName = "Alex Johnson",
            Initials = "AJ",
            IsOnline = true,
            LastSeen = DateTime.Now
        });
        
        // Add mock chat threads
        var johnContact = Contacts[0];
        var janeContact = Contacts[1];
        
        ChatThreads.Add(new ChatThreadModel
        {
            Id = Guid.NewGuid(),
            Contact = johnContact,
            LastMessageText = "Hi there! How are you today?",
            LastMessageTime = DateTime.Now.AddMinutes(-5),
            UnreadCount = 0
        });
        
        ChatThreads.Add(new ChatThreadModel
        {
            Id = Guid.NewGuid(),
            Contact = janeContact,
            LastMessageText = "Let's meet tomorrow",
            LastMessageTime = DateTime.Now.AddHours(-2),
            UnreadCount = 1
        });
        
        // Default current chat thread
        CurrentChatThread = ChatThreads[0];
        CurrentContact = johnContact;
    }

    /// <summary>
    /// Handle title bar mouse left button down to allow window dragging
    /// </summary>
    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            // Double click to maximize/restore
            MaximizeButton_Click(sender, e);
        }
        else
        {
            // Single click to drag
            DragMove();
        }
    }

    /// <summary>
    /// Handle minimize button click
    /// </summary>
    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Handle maximize/restore button click
    /// </summary>
    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
    }

    /// <summary>
    /// Handle close button click
    /// </summary>
    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    /// <summary>
    /// Handle switching to Chats tab
    /// </summary>
    private void ChatsTabButton_Click(object sender, RoutedEventArgs e)
    {
        // Update tab button styles
        ChatsTabButton.Foreground = FindResource("AccentBrightGreenBrush") as SolidColorBrush;
        ChatsTabButton.FontWeight = FontWeights.Bold;
        ContactsTabButton.Foreground = FindResource("NeutralGrayBrush") as SolidColorBrush;
        ContactsTabButton.FontWeight = FontWeights.Normal;
        
        // Show chats list, hide contacts list
        ChatsListView.Visibility = Visibility.Visible;
        ContactsListView.Visibility = Visibility.Collapsed;
        
        // Update state
        IsInChatMode = true;
    }
    
    /// <summary>
    /// Handle switching to Contacts tab
    /// </summary>
    private void ContactsTabButton_Click(object sender, RoutedEventArgs e)
    {
        // Update tab button styles
        ContactsTabButton.Foreground = FindResource("AccentBrightGreenBrush") as SolidColorBrush;
        ContactsTabButton.FontWeight = FontWeights.Bold;
        ChatsTabButton.Foreground = FindResource("NeutralGrayBrush") as SolidColorBrush;
        ChatsTabButton.FontWeight = FontWeights.Normal;
        
        // Show contacts list, hide chats list
        ContactsListView.Visibility = Visibility.Visible;
        ChatsListView.Visibility = Visibility.Collapsed;
        
        // Update state
        IsInChatMode = false;
    }
    
    /// <summary>
    /// Send a message and check for inappropriate content
    /// </summary>
    private async void SendMessageButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
            return;
        
        string originalMessage = MessageTextBox.Text;
        
        // Use AI to moderate the message
        string moderatedMessage = await _geminiService.ModerateChatMessageAsync(originalMessage);
        
        // Here you would normally add the message to your chat UI
        // For demo purposes, we'll just show a message box with the result
        if (originalMessage != moderatedMessage)
        {
            MessageBox.Show(
                $"Original: {originalMessage}\nModerated: {moderatedMessage}",
                "Message Moderated",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        
        // Clear the text box
        MessageTextBox.Clear();
    }
    
    // Model classes for storing data
    public class ContactModel
    {
        public Guid Id { get; set; }
        public string DisplayName { get; set; }
        public string Initials { get; set; }
        public bool IsOnline { get; set; }
        public DateTime LastSeen { get; set; }
    }
    
    public class ChatThreadModel
    {
        public Guid Id { get; set; }
        public ContactModel Contact { get; set; }
        public string LastMessageText { get; set; }
        public DateTime LastMessageTime { get; set; }
        public int UnreadCount { get; set; }
    }
}