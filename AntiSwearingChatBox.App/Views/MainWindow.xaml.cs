using System;
using System.Windows;
using System.Windows.Input;
using AntiSwearingChatBox.App.Services;
using Microsoft.Extensions.DependencyInjection;
using AntiSwearingChatBox.App.Components;

namespace AntiSwearingChatBox.App.Views
{
    public partial class MainWindow : Window
    {
        private readonly LoginPage _loginPage;
        private readonly RegisterPage _registerPage;
        private readonly ChatPage _chatPage;
        private readonly ApiService _apiService;
        
        public MainWindow()
        {
            InitializeComponent();
            
            _apiService = ((App)Application.Current).ServiceProvider.GetRequiredService<ApiService>();
            
            _loginPage = new LoginPage();
            _registerPage = new RegisterPage();
            _chatPage = new ChatPage();
            
            // Hook up login page events
            if (_loginPage.FindName("LoginComponent") is Login loginComponent)
            {
                loginComponent.LoginSuccessful += LoginComponent_LoginSuccessful;
                loginComponent.RegisterRequested += LoginComponent_RegisterRequested;
            }
            
            // Hook up register page events
            if (_registerPage.FindName("Register") is Register registerComponent)
            {
                registerComponent.BackToLoginRequested += RegisterComponent_BackToLoginRequested;
            }
            
            // Start with login page
            NavigateToLogin();
        }
        
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                //MaximizeButton_Click(sender, e);
            }
            else
            {
                DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        // Navigation methods with page caching
        public void NavigateToLogin()
        {
            MainFrame.Navigate(_loginPage);
        }
        
        public void NavigateToRegister()
        {
            MainFrame.Navigate(_registerPage);
        }
        
        public void NavigateToChat()
        {
            MainFrame.Navigate(_chatPage);
        }
        
        private void LoginComponent_LoginSuccessful(object? sender, EventArgs e)
        {
            NavigateToChat();
        }

        private void LoginComponent_RegisterRequested(object? sender, EventArgs e)
        {
            NavigateToRegister();
        }

        private void RegisterComponent_BackToLoginRequested(object? sender, EventArgs e)
        {
            NavigateToLogin();
        }
    }
} 