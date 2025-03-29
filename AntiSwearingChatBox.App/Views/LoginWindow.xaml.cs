using System;
using System.Windows;
using AntiSwearingChatBox.App.Services;

namespace AntiSwearingChatBox.App.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly AuthService _authService;

        public LoginWindow()
        {
            InitializeComponent();
            _authService = new AuthService("https://localhost:7001"); // Update with your server URL
            
            // Wire up button click events
            LoginButton.Click += LoginButton_Click;
            //btnRegister.Click += BtnRegister_Click;
            btnClose.Click += BtnClose_Click;
        }
        
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage.Text = "Please enter both username and password.";
                return;
            }

            try
            {
                LoginButton.IsEnabled = false;
                RegisterButton.IsEnabled = false;
                ErrorMessage.Text = "";

                var (success, message, token, refreshToken) = await _authService.LoginAsync(username, password);

                if (success)
                {
                    // Open main window and close login window
                    var mainWindow = new MainWindow();
                    mainWindow.Show();
                    this.Close();
                }
                else
                {
                    ErrorMessage.Text = message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Error: {ex.Message}";
            }
            finally
            {
                LoginButton.IsEnabled = true;
                RegisterButton.IsEnabled = true;
            }
        }
        
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.ShowDialog();
        }
        
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // Close the application
            Application.Current.Shutdown();
        }
    }
} 