using System;
using System.Windows;
using System.Windows.Input;
using AntiSwearingChatBox.WPF.Services;

namespace AntiSwearingChatBox.WPF.View
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
            
            _apiService = ServiceProvider.ApiService;
            
            _loginPage = new LoginPage();
            _registerPage = new RegisterPage();
            _chatPage = new ChatPage();
            
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
    }
} 