using System;
using System.Windows;
using System.Windows.Input;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AntiSwearingChatBox.App.Views
{
    public partial class LoginWindow : Window
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        
        // Keep track of currently shown panel for switching between login and register
        private bool _isRegistrationVisible = false;

        public LoginWindow()
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
                
                // Navigate to chat window and pass the authenticated user
                var chatWindow = new ChatWindow { Tag = user };
                chatWindow.Show();
                
                // Close login window
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Login error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        public void ShowRegisterPanel()
        {
            try
            {
                // Show registration panel - implementation would depend on your actual UI structure
                // For now, we'll just open a new RegisterWindow
                RegisterWindow registerWindow = new RegisterWindow();
                registerWindow.Show();
                this.Close();
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