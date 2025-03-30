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
        
        // Properties to expose username and password
        public string Username 
        { 
            get { return txtUsername.Text; }
        }
        
        public string Password 
        {
            get { return txtPassword.Password; }
        }
        
        public Login()
        {
            InitializeComponent();
            btnLogin.Click += BtnLogin_Click;
            btnRegister.MouseDown += BtnRegister_Click;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                MessageBox.Show("Please enter both username and password.", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            // Raise the login successful event
            LoginSuccessful?.Invoke(this, EventArgs.Empty);
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Get parent window
            Window parentWindow = Window.GetWindow(this);
            
            // Check if we're in the new Page-based navigation
            if (parentWindow is MainWindow && this.Parent != null)
            {
                // Find containing page
                var parent = this.Parent;
                while (parent != null && !(parent is Page))
                {
                    if (parent is FrameworkElement fe)
                    {
                        parent = fe.Parent;
                    }
                    else
                    {
                        break;
                    }
                }
                
                if (parent is LoginPage loginPage)
                {
                    loginPage.ShowRegisterPanel();
                    return;
                }
            }
        }
    }
}
