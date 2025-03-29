using System;
using System.Windows;
using System.Windows.Input;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AntiSwearingChatBox.App.Views
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private readonly IUserService _userService;
        private readonly Components.Register _registerComponent;

        public RegisterWindow()
        {
            InitializeComponent();
            
            // Get services from DI container
            var app = (App)Application.Current;
            _userService = app.ServiceProvider.GetRequiredService<IUserService>();
            
            // Get reference to the Register component
            _registerComponent = this.FindName("registerComponent") as Components.Register;
            
            // Subscribe to register button click event if available
            if (_registerComponent != null)
            {
                _registerComponent.RegisterButtonClicked += RegisterComponent_RegisterButtonClicked;
            }
        }
        
        private void RegisterComponent_RegisterButtonClicked(object sender, 
            (string username, string email, string password, string confirmPassword) formData)
        {
            try
            {
                // Use registration info from the component
                string username = formData.username;
                string email = formData.email;
                string password = formData.password;
                string confirmPassword = formData.confirmPassword;
                
                // Basic validation
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || 
                    string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
                {
                    MessageBox.Show("Please fill in all fields.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                if (password != confirmPassword)
                {
                    MessageBox.Show("Passwords do not match.", "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Create new user
                var user = new User
                {
                    Username = username,
                    Email = email,
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = DateTime.Now,
                    Role = "User",
                    TrustScore = 1.0m
                };
                
                // Register user
                var result = _userService.Register(user, password);
                
                if (!result.success)
                {
                    MessageBox.Show(result.message, "Registration Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Registration successful
                MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                
                // Back to login window
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Registration error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackToLoginButton_Click(object sender, RoutedEventArgs e)
        {
            // Return to login window
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
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
    }
}