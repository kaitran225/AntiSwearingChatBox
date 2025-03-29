using System;
using System.Windows;
using AntiSwearingChatBox.Service.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace AntiSwearingChatBox.App.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private readonly IMessageHistoryService _messageHistoryService;
        
        public LoginWindow(IMessageHistoryService messageHistoryService)
        {
            _messageHistoryService = messageHistoryService;
            InitializeComponent();
            
            // Subscribe to login events from the login component
            var loginComponent = this.FindName("LoginComponent") as Components.Login;
            if (loginComponent != null)
            {
                loginComponent.LoginSuccessful += LoginComponent_LoginSuccessful;
            }
        }

        private void Footer_Loaded(object sender, RoutedEventArgs e)
        {
            // Any initialization code for the footer
        }
        
        private void LoginComponent_LoginSuccessful(object sender, EventArgs e)
        {
            // Open the chat window
            var chatWindow = new ChatWindow(_messageHistoryService);
            chatWindow.Show();
            
            // Close the login window
            this.Close();
        }
    }
}