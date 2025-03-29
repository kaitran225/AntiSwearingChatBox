using System;
using System.Windows;
using AntiSwearingChatBox.App.Services;

namespace AntiSwearingChatBox.App.Views
{
    /// <summary>
    /// Interaction logic for RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        private readonly AuthService _authService;

        public RegisterWindow()
        {
            InitializeComponent();
            _authService = new AuthService("https://localhost:7001"); // Update with your server URL
        }

        private async void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var username = UsernameTextBox.Text;
            var email = EmailTextBox.Text;
            var password = PasswordBox.Password;
            var confirmPassword = ConfirmPasswordBox.Password;
            var gender = GenderComboBox.SelectedItem as ComboBoxItem;

            // Validate inputs
            if (string.IsNullOrWhiteSpace(username))
            {
                ErrorMessage.Text = "Please enter a username.";
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                ErrorMessage.Text = "Please enter an email address.";
                return;
            }

            if (!IsValidEmail(email))
            {
                ErrorMessage.Text = "Please enter a valid email address.";
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage.Text = "Please enter a password.";
                return;
            }

            if (password.Length < 6)
            {
                ErrorMessage.Text = "Password must be at least 6 characters long.";
                return;
            }

            if (password != confirmPassword)
            {
                ErrorMessage.Text = "Passwords do not match.";
                return;
            }

            try
            {
                RegisterButton.IsEnabled = false;
                CancelButton.IsEnabled = false;
                ErrorMessage.Text = "";

                var (success, message) = await _authService.RegisterAsync(
                    username,
                    email,
                    password,
                    gender?.Content.ToString());

                if (success)
                {
                    MessageBox.Show(
                        "Registration successful! Please check your email to verify your account.",
                        "Success",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    this.Close();
                }
                else
                {
                    ErrorMessage.Text = message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage.Text = $"Error: {ex.Message}";
            }
            finally
            {
                RegisterButton.IsEnabled = true;
                CancelButton.IsEnabled = true;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
} 