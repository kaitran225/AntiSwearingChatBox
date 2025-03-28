using System;
using System.Windows;

namespace AntiSwearingChatBox.App.Views
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            
            // Wire up button click events
            btnLogin.Click += BtnLogin_Click;
            //btnRegister.Click += BtnRegister_Click;
            btnClose.Click += BtnClose_Click;
        }
        
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add proper authentication logic
            
            // For now, just open the main window to showcase the UI
            ChatWindow chatWindow = new ChatWindow();
            chatWindow.Show();
            
            // Close the login window
            this.Close();
        }
        
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Open register window
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
            
            // Close the login window
            this.Close();
        }
        
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // Close the application
            Application.Current.Shutdown();
        }
    }
} 