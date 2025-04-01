using System;
using System.Windows;
using System.Windows.Input;
using AntiSwearingChatBox.WPF.Commands;
using AntiSwearingChatBox.WPF.Services;
using AntiSwearingChatBox.WPF.Views;

namespace AntiSwearingChatBox.WPF.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _email = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isLoading;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
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

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand RegisterCommand { get; }
        public ICommand NavigateToLoginCommand { get; }

        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(ExecuteRegisterAsync, CanRegister);
            NavigateToLoginCommand = new RelayCommand(ExecuteNavigateToLogin);
        }

        private bool CanRegister()
        {
            return !IsLoading &&
                   !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !string.IsNullOrWhiteSpace(ConfirmPassword);
        }

        private async void ExecuteRegisterAsync()
        {
            try
            {
                if (Password != ConfirmPassword)
                {
                    ErrorMessage = "Passwords do not match.";
                    return;
                }

                ErrorMessage = string.Empty;
                IsLoading = true;

                var response = await ServiceProvider.ApiService.RegisterAsync(Username, Email, Password);

                if (response.Success)
                {
                    MessageBox.Show("Registration successful! Please log in.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // Navigate to login
                    ExecuteNavigateToLogin();
                }
                else
                {
                    ErrorMessage = response.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Registration error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteNavigateToLogin()
        {
            var loginView = new LoginView();
            loginView.Show();
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow()
        {
            if (Application.Current.Windows[0] is Window window)
            {
                window.Close();
            }
        }
    }
} 