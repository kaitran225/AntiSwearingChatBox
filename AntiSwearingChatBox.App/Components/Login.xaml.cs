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
        
        // Add event for login success
        public event EventHandler LoginSuccessful;
        
        // Properties to expose username and password
        public string Username 
        { 
            get { return txtUsername.Text; }
        }
        
        public string Password 
        {
            get { return txtPassword.Password; }
        }
        
        public Login()
        {
            InitializeComponent();
            btnLogin.Click += BtnLogin_Click;
            btnRegister.MouseDown += BtnRegister_Click;
            
            // Get ApiService from DI container
            _apiService = ((App)Application.Current).ServiceProvider.GetService<ApiService>();
        }

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Disable login button during API call
            btnLogin.IsEnabled = false;
            loadingIndicator.Visibility = Visibility.Visible;
            
            try
            {
                // Call API to login
                var (success, token, message) = await _apiService.LoginAsync(Username, Password);
                
                if (success)
                {
                    // Save token and raise success event
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
                btnLogin.IsEnabled = true;
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Get parent window
            Window parentWindow = Window.GetWindow(this);
            
            // Check if we're in the new Page-based navigation
            if (parentWindow is MainWindow && this.Parent != null)
            {
                // Find containing page
                var parent = this.Parent;
                while (parent != null && !(parent is Page))
                {
                    if (parent is FrameworkElement fe)
                    {
                        parent = fe.Parent;
                    }
                    else
                    {
                        break;
                    }
                }
                
                if (parent is LoginPage loginPage)
                {
                    loginPage.ShowRegisterPanel();
                    return;
                }
            }
            
            // If we got here, we couldn't find the login page, so navigate using the main window
            if (parentWindow is MainWindow mainWindow)
            {
                mainWindow.NavigateToRegister();
            }
        }
    }
}
