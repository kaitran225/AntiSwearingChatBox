using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.App.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AntiSwearingChatBox.App.Components
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Register : UserControl
    {
        public Register()
        {
            InitializeComponent();
            btnLogin.MouseDown += BtnLogin_Click;
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Get the service provider from the application
            var app = Application.Current as App;
            if (app != null)
            {
                // Get the login window from the service provider
                var loginWindow = app.ServiceProvider.GetService<LoginWindow>();
                if (loginWindow != null)
                {
                    loginWindow.Show();
                    
                    Window parentWindow = Window.GetWindow(this); // Get the parent window
                    parentWindow?.Close();
                }
                else
                {
                    MessageBox.Show("Could not create login window from service provider.");
                }
            }
            else
            {
                MessageBox.Show("Application context is not available.");
            }
        }
    }
}
