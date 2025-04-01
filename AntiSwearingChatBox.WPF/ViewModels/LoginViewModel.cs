using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AntiSwearingChatBox.WPF.Commands;
using AntiSwearingChatBox.WPF.Services;

namespace AntiSwearingChatBox.WPF.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _username = string.Empty;
        private string _password = string.Empty;
        private bool _isLoggingIn;
        private string _errorMessage = string.Empty;

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                OnPropertyChanged();
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoggingIn
        {
            get => _isLoggingIn;
            set
            {
                _isLoggingIn = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }

        public LoginViewModel()
        {
            _apiService = ServiceProvider.ApiService;
            LoginCommand = new RelayCommand(ExecuteLoginAsync, CanLogin);
            RegisterCommand = new RelayCommand(ExecuteRegister);
        }

        private bool CanLogin()
        {
            return !string.IsNullOrWhiteSpace(Username) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   !IsLoggingIn;
        }

        private async void ExecuteLoginAsync()
        {
            try
            {
                IsLoggingIn = true;
                ErrorMessage = string.Empty;

                var result = await _apiService.LoginAsync(Username, Password);
                if (result.success)
                {
                    // Login successful, navigate to chat view
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Directly access current window and close it
                        if (Application.Current.MainWindow is Window currentWindow)
                        {
                            currentWindow.Close();
                        }
                    });
                }
                else
                {
                    ErrorMessage = result.message;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoggingIn = false;
            }
        }

        private void ExecuteRegister()
        {
            // Navigate to Register view
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Open the register window
                MessageBox.Show("Register functionality not implemented.");
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 