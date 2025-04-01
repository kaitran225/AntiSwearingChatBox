using System;
using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.WPF.Components;

namespace AntiSwearingChatBox.WPF.View
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            
            // Connect to the Login component's events
            if (LoginComponent != null)
            {
                LoginComponent.LoginSuccessful += LoginComponent_LoginSuccessful!;
                LoginComponent.RegisterRequested += LoginComponent_RegisterRequested!;
            }
        }

        private void LoginComponent_LoginSuccessful(object sender, EventArgs e)
        {
            // Navigate to chat page
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToChat();
            }
        }
        
        private void LoginComponent_RegisterRequested(object sender, EventArgs e)
        {
            // Navigate to registration page
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToRegister();
            }
        }
        
        private void Footer_Loaded(object sender, RoutedEventArgs e)
        {
            // Add any footer initialization logic here
        }

        public void ShowRegisterPanel()
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToRegister();
            }
        }
    }
} 