using System;
using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AntiSwearingChatBox.App.Views
{
    public partial class LoginPage : Page
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public LoginPage()
        {
            InitializeComponent();
            
            // Get services from DI container
            var app = (App)Application.Current;
            _userService = app.ServiceProvider.GetRequiredService<IUserService>();
            _authService = app.ServiceProvider.GetRequiredService<IAuthService>();
            
            // Subscribe to login event
            LoginComponent.LoginSuccessful += LoginComponent_LoginSuccessful;
        }

        private void LoginComponent_LoginSuccessful(object sender, EventArgs e)
        {
            try
            {
                string username = LoginComponent.Username;
                string password = LoginComponent.Password;
                
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Please enter both username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Authenticate the user
                var result = _userService.Authenticate(username, password);
                
                if (!result.success || result.user == null)
                {
                    MessageBox.Show(result.message, "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // User authenticated successfully
                User user = result.user;
                
                // Store the user in application state or pass to the next page
                var app = (App)Application.Current;
                app.CurrentUser = user;
                
                // Navigate to chat page
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.NavigateToChat();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        public void ShowRegisterPanel()
        {
            try
            {
                // Navigate to register page
                if (Window.GetWindow(this) is MainWindow mainWindow)
                {
                    mainWindow.NavigateToRegister();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing registration panel: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void Footer_Loaded(object sender, RoutedEventArgs e)
        {
            // Add any footer initialization logic here
        }
    }
} 