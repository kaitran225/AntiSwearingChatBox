using System;
using System.Windows;
using System.Windows.Controls;

namespace AntiSwearingChatBox.WPF.View
{
    public partial class RegisterPage : Page
    {
        public RegisterPage()
        {
            InitializeComponent();
            
            // Connect to Register component's events
            if (registerComponent != null)
            {
                registerComponent.BackToLoginRequested += RegisterComponent_BackToLoginRequested;
            }
        }
        
        private void RegisterComponent_RegisterButtonClicked(object sender, (string username, string email, string password, string confirmPassword) formData)
        {
            // Empty implementation
        }
        
        private void RegisterComponent_BackToLoginRequested(object sender, EventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToLogin();
            }
        }
    }
} 