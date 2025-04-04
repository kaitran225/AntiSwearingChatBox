using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AntiSwearingChatBox.WPF.Services.Api;

namespace AntiSwearingChatBox.WPF.Components
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : UserControl
    {
        private readonly ApiService _apiService;
        private readonly TextBox _txtUsername;
        private readonly TextBox _txtEmail;
        private readonly PasswordBox _txtPassword;
        private readonly PasswordBox _txtConfirmPassword;
        private readonly Button _btnRegister;
        private readonly TextBlock _btnLogin;
        private readonly TextBlock _passwordStrengthText;
        private readonly Border _strengthBar1;
        private readonly Border _strengthBar2;
        private readonly Border _strengthBar3;
        private readonly Border _strengthBar4;

        public event EventHandler? BackToLoginRequested;

        public Register()
        {
            InitializeComponent();
            _apiService = (ApiService)Services.ServiceProvider.ApiService;
            _txtUsername = (TextBox)FindName("txtUsername") ?? throw new InvalidOperationException("txtUsername not found");
            _txtEmail = (TextBox)FindName("txtEmail") ?? throw new InvalidOperationException("txtEmail not found");
            _txtPassword = (PasswordBox)FindName("txtPassword") ?? throw new InvalidOperationException("txtPassword not found");
            _txtConfirmPassword = (PasswordBox)FindName("txtConfirmPassword") ?? throw new InvalidOperationException("txtConfirmPassword not found");
            _btnRegister = (Button)FindName("btnRegister") ?? throw new InvalidOperationException("btnRegister not found");
            _btnLogin = (TextBlock)FindName("btnLogin") ?? throw new InvalidOperationException("btnLogin not found");
            _passwordStrengthText = (TextBlock)FindName("passwordStrengthText") ?? throw new InvalidOperationException("passwordStrengthText not found");
            _strengthBar1 = (Border)FindName("strengthBar1") ?? throw new InvalidOperationException("strengthBar1 not found");
            _strengthBar2 = (Border)FindName("strengthBar2") ?? throw new InvalidOperationException("strengthBar2 not found");
            _strengthBar3 = (Border)FindName("strengthBar3") ?? throw new InvalidOperationException("strengthBar3 not found");
            _strengthBar4 = (Border)FindName("strengthBar4") ?? throw new InvalidOperationException("strengthBar4 not found");

            _btnRegister.Click += BtnRegister_Click;
            _btnLogin.MouseDown += BtnLogin_Click;
            _txtPassword.PasswordChanged += TxtPassword_PasswordChanged;
        }

        private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            UpdatePasswordStrength(_txtPassword.Password);
        }

        private void UpdatePasswordStrength(string password)
        {
            int strength = CalculatePasswordStrength(password);
            SolidColorBrush greenBrush = (SolidColorBrush)FindResource("PrimaryGreenBrush");
            SolidColorBrush orangeBrush = new SolidColorBrush(Color.FromRgb(255, 165, 0));
            SolidColorBrush redBrush = new SolidColorBrush(Color.FromRgb(220, 53, 69));
            SolidColorBrush grayBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51));

            // Reset all bars to gray
            _strengthBar1.Background = grayBrush;
            _strengthBar2.Background = grayBrush;
            _strengthBar3.Background = grayBrush;
            _strengthBar4.Background = grayBrush;

            // Update text and color based on strength
            if (string.IsNullOrEmpty(password))
            {
                _passwordStrengthText.Text = "None";
                _passwordStrengthText.Foreground = grayBrush;
                return;
            }

            switch (strength)
            {
                case 1:
                    _passwordStrengthText.Text = "Weak";
                    _passwordStrengthText.Foreground = redBrush;
                    _strengthBar1.Background = redBrush;
                    break;
                case 2:
                    _passwordStrengthText.Text = "Fair";
                    _passwordStrengthText.Foreground = orangeBrush;
                    _strengthBar1.Background = orangeBrush;
                    _strengthBar2.Background = orangeBrush;
                    break;
                case 3:
                    _passwordStrengthText.Text = "Good";
                    _passwordStrengthText.Foreground = greenBrush;
                    _strengthBar1.Background = greenBrush;
                    _strengthBar2.Background = greenBrush;
                    _strengthBar3.Background = greenBrush;
                    break;
                case 4:
                    _passwordStrengthText.Text = "Strong";
                    _passwordStrengthText.Foreground = greenBrush;
                    _strengthBar1.Background = greenBrush;
                    _strengthBar2.Background = greenBrush;
                    _strengthBar3.Background = greenBrush;
                    _strengthBar4.Background = greenBrush;
                    break;
            }
        }

        private int CalculatePasswordStrength(string password)
        {
            if (string.IsNullOrEmpty(password))
                return 0;

            int score = 0;
            
            // Length check
            if (password.Length >= 8)
                score += 1;
            
            // Complexity checks
            if (password.Length >= 10)
                score += 1;
                
            bool hasUpperCase = false;
            bool hasLowerCase = false;
            bool hasDigit = false;
            bool hasSpecialChar = false;
            
            foreach (char c in password)
            {
                if (char.IsUpper(c)) hasUpperCase = true;
                else if (char.IsLower(c)) hasLowerCase = true;
                else if (char.IsDigit(c)) hasDigit = true;
                else hasSpecialChar = true;
            }
            
            int complexityScore = 0;
            if (hasUpperCase) complexityScore++;
            if (hasLowerCase) complexityScore++;
            if (hasDigit) complexityScore++;
            if (hasSpecialChar) complexityScore++;
            
            if (complexityScore >= 3)
                score += 1;
                
            if (complexityScore >= 4 && password.Length >= 12)
                score += 1;
                
            return Math.Min(4, score);
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
                var response = await _apiService.RegisterAsync(Username, Email, Password);
                
                if (response.Success)
                {
                    MessageBox.Show("Registration successful! Please log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    BackToLoginRequested?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    MessageBox.Show($"Registration failed: {response.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
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
