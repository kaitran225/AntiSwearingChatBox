using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace AntiSwearingChatBox.WPF.View
{
    public partial class ChatView : Page
    {
        private readonly HttpClient _httpClient;
        private ObservableCollection<ChatMessage> _messages;
        private DispatcherTimer _messageUpdateTimer;
        private const string BaseUrl = "http://localhost:5000/api"; // Update this with your actual API URL

        public ChatView()
        {
            InitializeComponent();
            
            _httpClient = new HttpClient();
            _messages = new ObservableCollection<ChatMessage>();
            MessagesItemsControl.ItemsSource = _messages;
            
            _messageUpdateTimer = new DispatcherTimer();
            _messageUpdateTimer.Interval = TimeSpan.FromSeconds(2);
            _messageUpdateTimer.Tick += MessageUpdateTimer_Tick;
            _messageUpdateTimer.Start();

            // Load initial messages
            LoadMessages();

            // Set username
            UsernameTextBlock.Text = "User"; // Default username
        }

        private async void LoadMessages()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{BaseUrl}/messages");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var messages = JsonSerializer.Deserialize<ChatMessage[]>(content);
                    
                    _messages.Clear();
                    foreach (var message in messages)
                    {
                        _messages.Add(message);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load messages: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void MessageUpdateTimer_Tick(object sender, EventArgs e)
        {
            LoadMessages();
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            await SendMessage();
        }

        private async void MessageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                e.Handled = true;
                await SendMessage();
            }
        }

        private async Task SendMessage()
        {
            if (string.IsNullOrWhiteSpace(MessageTextBox.Text))
                return;

            try
            {
                var messageData = new
                {
                    Content = MessageTextBox.Text,
                    Username = UsernameTextBlock.Text
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(messageData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/messages", content);
                
                if (response.IsSuccessStatusCode)
                {
                    MessageTextBox.Clear();
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Failed to send message: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to login
            NavigationService?.Navigate(new Uri("/Views/LoginView.xaml", UriKind.Relative));
        }
    }

    public class ChatMessage
    {
        public string Username { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }
    }
} 