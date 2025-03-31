using System;
using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.App.Views;
using AntiSwearingChatBox.App.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AntiSwearingChatBox.App.Components
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        private readonly ApiService _apiService;
        
        public event EventHandler? LoginSuccessful;
        public event EventHandler? RegisterRequested;
        
        private TextBox? _txtUsername;
        private PasswordBox? _txtPassword;
        private Button? _btnLogin;
        private TextBlock? _btnRegister;
        
        public string Username => _txtUsername?.Text ?? string.Empty;
        public string Password => _txtPassword?.Password ?? string.Empty;
        
        public Login()
        {
            InitializeComponent();
            
            // Get ApiService from DI container
            _apiService = ((App)Application.Current).ServiceProvider.GetRequiredService<ApiService>();
            
            // Find controls
            _txtUsername = FindName("txtUsername") as TextBox;
            _txtPassword = FindName("txtPassword") as PasswordBox;
            _btnLogin = FindName("btnLogin") as Button;
            _btnRegister = FindName("btnRegister") as TextBlock;
            
            // Hook up events
            if (_btnLogin != null)
                _btnLogin.Click += BtnLogin_Click;
            if (_btnRegister != null)
                _btnRegister.MouseDown += BtnRegister_Click;
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            if (_btnLogin != null)
                _btnLogin.IsEnabled = false;
            
            try
            {
                var (success, token, message) = await _apiService.LoginAsync(Username, Password);
                
                if (success)
                {
                    LoginSuccessful?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show(message, "Login Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                if (_btnLogin != null)
                    _btnLogin.IsEnabled = true;
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RegisterRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
