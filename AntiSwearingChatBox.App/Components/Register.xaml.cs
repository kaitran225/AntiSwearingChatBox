using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.App.Views;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AntiSwearingChatBox.App.Components
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : UserControl
    {
        // Event for registration
        public event EventHandler<(string username, string email, string password, string confirmPassword)> RegisterButtonClicked;
        
        // Controls to expose
        private TextBox _txtUsername;
        private TextBox _txtEmail;
        private PasswordBox _txtPassword;
        private PasswordBox _txtConfirmPassword;
        
        public Register()
        {
            InitializeComponent();
            
            // Hook up to login link
            btnLogin.MouseDown += BtnLogin_Click;
            
            // Get references to form controls
            _txtUsername = this.FindName("txtUsername") as TextBox;
            _txtEmail = this.FindName("txtEmail") as TextBox;
            _txtPassword = this.FindName("txtPassword") as PasswordBox;
            _txtConfirmPassword = this.FindName("txtConfirmPassword") as PasswordBox;
            
            // Hook up register button click event
            Button registerButton = this.FindName("btnRegister") as Button;
            if (registerButton != null)
            {
                registerButton.Click += BtnRegister_Click;
            }
        }
        
        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Validate inputs and raise event
            string username = _txtUsername?.Text ?? string.Empty;
            string email = _txtEmail?.Text ?? string.Empty;
            string password = _txtPassword?.Password ?? string.Empty;
            string confirmPassword = _txtConfirmPassword?.Password ?? string.Empty;
            
            // Raise the registration event
            RegisterButtonClicked?.Invoke(this, (username, email, password, confirmPassword));
        }
        
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
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
                
                // If parent is RegisterPage2, navigate back to login
                if (parent is RegisterPage2 registerPage)
                {
                    if (parentWindow is MainWindow mainWindow)
                    {
                        mainWindow.NavigateToLogin();
                        return;
                    }
                }
            }
            
            // Legacy window-based approach
            // Get the service provider from the application
            var app = Application.Current as App;
            if (app != null)
            {
                // Get the login window from the service provider
                var loginWindow = app.ServiceProvider.GetService<LoginWindow>();
                if (loginWindow != null)
                {
                    loginWindow.Show();
                    
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
