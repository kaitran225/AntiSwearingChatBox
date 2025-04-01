using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AntiSwearingChatBox.WPF.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AntiSwearingChatBox.WPF.Components
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : UserControl
    {
        //private readonly ApiService _apiService;
        private readonly TextBox _txtUsername;
        private readonly TextBox _txtEmail;
        private readonly PasswordBox _txtPassword;
        private readonly PasswordBox _txtConfirmPassword;
        private readonly Button _btnRegister;
        private readonly TextBlock _btnLogin;

        public event EventHandler? BackToLoginRequested;

        public Register()
        {
            InitializeComponent();
            //_apiService = ((App)Application.Current).ServiceProvider.GetRequiredService<ApiService>();

            _txtUsername = (TextBox)FindName("txtUsername") ?? throw new InvalidOperationException("txtUsername not found");
            _txtEmail = (TextBox)FindName("txtEmail") ?? throw new InvalidOperationException("txtEmail not found");
            _txtPassword = (PasswordBox)FindName("txtPassword") ?? throw new InvalidOperationException("txtPassword not found");
            _txtConfirmPassword = (PasswordBox)FindName("txtConfirmPassword") ?? throw new InvalidOperationException("txtConfirmPassword not found");
            _btnRegister = (Button)FindName("btnRegister") ?? throw new InvalidOperationException("btnRegister not found");
            _btnLogin = (TextBlock)FindName("btnLogin") ?? throw new InvalidOperationException("btnLogin not found");

            _btnRegister.Click += BtnRegister_Click;
            _btnLogin.MouseDown += BtnLogin_Click;
        }

        private async void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Email) || 
                string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Password != ConfirmPassword)
            {
                MessageBox.Show("Passwords do not match.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _btnRegister.IsEnabled = false;
                //var (success, message) = await _apiService.RegisterAsync(Username, Email, Password);
                
                //if (success)
                //{
                //    MessageBox.Show("Registration successful! Please log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                //    BackToLoginRequested?.Invoke(this, EventArgs.Empty);
                //}
                //else
                //{
                //    MessageBox.Show($"Registration failed: {message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //}
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                _btnRegister.IsEnabled = true;
            }
        }

        private void BtnLogin_Click(object sender, MouseButtonEventArgs e)
        {
            BackToLoginRequested?.Invoke(this, EventArgs.Empty);
        }

        private string Username => _txtUsername.Text;
        private string Email => _txtEmail.Text;
        private string Password => _txtPassword.Password;
        private string ConfirmPassword => _txtConfirmPassword.Password;
    }
}
