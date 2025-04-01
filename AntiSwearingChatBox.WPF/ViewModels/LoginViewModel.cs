using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AntiSwearingChatBox.WPF.Commands;
using AntiSwearingChatBox.WPF.Services.Api;
using AntiSwearingChatBox.WPF.View;

namespace AntiSwearingChatBox.WPF.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly IApiService _apiService;
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
            // Get the proper API service from DI
            _apiService = new ApiService();
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
                if (result.Success)
                {
                    try
                    {
                        // Try to connect to the SignalR hub
                        await _apiService.ConnectToHubAsync();
                    }
                    catch (Exception ex)
                    {
                        // Log but continue - we don't want to block login if real-time isn't available
                        Console.WriteLine($"Warning: Could not connect to chat hub: {ex.Message}");
                    }
                    
                    // Login successful, navigate to chat view
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Navigate to the chat page
                        if (Application.Current.MainWindow is View.MainWindow mainWindow)
                        {
                            mainWindow.NavigateToChat();
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
                if (Application.Current.MainWindow is View.MainWindow mainWindow)
                {
                    mainWindow.NavigateToRegister();
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 