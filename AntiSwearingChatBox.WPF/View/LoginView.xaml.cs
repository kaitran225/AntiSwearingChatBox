using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace AntiSwearingChatBox.WPF.View
{
    public partial class LoginView : Page
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "http://localhost:5000/api"; // Update this with your actual API URL

        public LoginView()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var loginData = new
                {
                    Username = UsernameTextBox.Text,
                    Password = PasswordBox.Password
                };

                var content = new StringContent(
                    JsonSerializer.Serialize(loginData),
                    Encoding.UTF8,
                    "application/json");

                var response = await _httpClient.PostAsync($"{BaseUrl}/auth/login", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var token = await response.Content.ReadAsStringAsync();
                    // Store the token for future API calls (using a different approach without App.Current)
                    
                    // Navigate to the main chat view
                    NavigationService?.Navigate(new Uri("/Views/ChatView.xaml", UriKind.Relative));
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Login failed: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to the registration view
            NavigationService?.Navigate(new Uri("/Views/RegisterView.xaml", UriKind.Relative));
        }
    }
} 