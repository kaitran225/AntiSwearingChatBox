using System;
using System.Windows;

namespace AntiSwearingChatBox.App.Views
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
            btnLogin.MouseDown += BtnLogin_Click;
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add proper authentication logic

            // For now, just open the main window to showcase the UI
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            // Close the login window
            this.Close();
        }
    }
}