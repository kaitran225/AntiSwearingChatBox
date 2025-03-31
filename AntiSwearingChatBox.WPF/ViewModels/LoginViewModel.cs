using System;
using System.Windows;
using System.Windows.Input;
using AntiSwearingChatBox.WPF.Commands;
using AntiSwearingChatBox.WPF.Services;
using AntiSwearingChatBox.WPF.Views;

namespace AntiSwearingChatBox.WPF.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username = string.Empty;
        private string _password = string.Empty;
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

        public ICommand LoginCommand { get; }
        public ICommand NavigateToRegisterCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLoginAsync, CanLogin);
            NavigateToRegisterCommand = new RelayCommand(ExecuteNavigateToRegister);
        }

        private bool CanLogin()
        {
            return !IsLoading && !string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password);
        }

        private async void ExecuteLoginAsync()
        {
            try
            {
                ErrorMessage = string.Empty;
                IsLoading = true;

                var response = await ServiceProvider.ApiService.LoginAsync(Username, Password);

                if (response.Success)
                {
                    // Open chat window
                    var chatView = new ChatView(response.Username);
                    chatView.Show();

                    // Close login window
                    CloseCurrentWindow();
                }
                else
                {
                    ErrorMessage = response.Message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteNavigateToRegister()
        {
            var registerView = new RegisterView();
            registerView.Show();
            CloseCurrentWindow();
        }

        private void CloseCurrentWindow()
        {
            if (Application.Current.MainWindow is Window window)
            {
                window.Close();
            }
        }
    }
} 