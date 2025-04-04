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
        private readonly SimpleChatPage _chatPage;
        private readonly AITestPage _aiTestPage;

        
        public MainWindow()
        {
            InitializeComponent();
            
            _loginPage = new LoginPage();
            _registerPage = new RegisterPage();
            _chatPage = new SimpleChatPage();
            _aiTestPage = new AITestPage();
            
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
        
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    DragMove();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in window drag: {ex.Message}");
            }
        }
        
        // Navigation methods with page caching
        public void NavigateToLogin()
        {
            MainFrame.Navigate(_loginPage);
            try
            {
                // Set window title directly
                this.Title = "Login - Anti-Swearing Chat Box";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting window title: {ex.Message}");
            }
        }
        
        public void NavigateToRegister()
        {
            MainFrame.Navigate(_registerPage);
            try
            {
                // Set window title directly
                this.Title = "Register - Anti-Swearing Chat Box";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting window title: {ex.Message}");
            }
        }
        
        public void NavigateToChat()
        {
            MainFrame.Navigate(_chatPage);
            try
            {
                // Set window title directly
                this.Title = "Chat - Anti-Swearing Chat Box";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting window title: {ex.Message}");
            }
        }
        
        public void NavigateToAITest()
        {
            MainFrame.Navigate(_aiTestPage);
            try
            {
                // Set window title directly
                this.Title = "AI Testing - Anti-Swearing Chat Box";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error setting window title: {ex.Message}");
            }
        }
    }
} 