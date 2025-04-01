using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AntiSwearingChatBox.WPF.Commands;
using AntiSwearingChatBox.WPF.Services;
using AntiSwearingChatBox.WPF.Services.Api;
using AntiSwearingChatBox.WPF.View;

namespace AntiSwearingChatBox.WPF.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IApiService _apiService;
        private string _username = string.Empty;
        private string _email = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private bool _isRegistering;
        private string _errorMessage = string.Empty;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public bool IsRegistering
        {
            get => _isRegistering;
            set => SetProperty(ref _isRegistering, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand BackCommand { get; }

        public RegisterViewModel()
        {
            _apiService = AntiSwearingChatBox.WPF.Services.ServiceProvider.ApiService;
            RegisterCommand = new RelayCommand(ExecuteRegisterAsync, CanRegister);
            BackCommand = new RelayCommand(ExecuteBack);
        }

        private bool CanRegister()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword) &&
                   Password == ConfirmPassword &&
                   !IsRegistering;
        }

        private async void ExecuteRegisterAsync()
        {
            try
            {
                IsRegistering = true;
                ErrorMessage = string.Empty;

                var result = await _apiService.RegisterAsync(Username, Email, Password);
                if (result.Success)
                {
                    // Registration successful, notify the user
                    MessageBox.Show("Registration successful! You can now log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Navigate to login page
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (Application.Current.MainWindow is View.MainWindow mainWindow)
                        {
                            mainWindow.NavigateToLogin();
                        }
                    });
                }
                else
                {
                    ErrorMessage = result.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration failed: {ex.Message}";
            }
            finally
            {
                IsRegistering = false;
            }
        }

        private void ExecuteBack()
        {
            // Navigate back to login page
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow is View.MainWindow mainWindow)
                {
                    mainWindow.NavigateToLogin();
                }
            });
        }
    }
} 