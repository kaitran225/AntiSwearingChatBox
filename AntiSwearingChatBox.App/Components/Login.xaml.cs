using System;
using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.App.Views;

namespace AntiSwearingChatBox.App.Components
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        // Add event for login success
        public event EventHandler LoginSuccessful;
        
        public Login()
        {
            InitializeComponent();
            btnLogin.Click += BtnLogin_Click;
            btnRegister.MouseDown += BtnRegister_Click;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add proper authentication logic

            // For now, simulate a successful login
            // Raise the login successful event instead of directly creating window
            LoginSuccessful?.Invoke(this, EventArgs.Empty);
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Open register window - this would be replaced with an event as well
            // in a more complete implementation
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();

            // Close the login window
            Window parentWindow = Window.GetWindow(this); // Get the parent window
            parentWindow?.Close();
        }
    }
}
