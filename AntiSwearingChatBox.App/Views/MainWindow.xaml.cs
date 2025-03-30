using System.Windows;
using System.Windows.Input;

namespace AntiSwearingChatBox.App.Views
{
    public partial class MainWindow : Window
    {
        // Store page instances for better state management
        private LoginPage _loginPage;
        private RegisterPage _registerPage;
        private ChatPage _chatPage;
        
        public MainWindow()
        {
            InitializeComponent();
            
            // Navigate to the login page by default
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
            // Create a new login page or reuse the existing one
            if (_loginPage == null)
            {
                _loginPage = new LoginPage();
            }
            
            MainFrame.Navigate(_loginPage);
            UpdateTitle("Login");
        }
        
        public void NavigateToRegister()
        {
            // Create a new register page or reuse the existing one
            if (_registerPage == null)
            {
                _registerPage = new RegisterPage();
            }
            
            MainFrame.Navigate(_registerPage);
            UpdateTitle("Register");
        }
        
        public void NavigateToChat()
        {
            // Create a new chat page or reuse the existing one
            if (_chatPage == null)
            {
                _chatPage = new ChatPage();
            }
            
            MainFrame.Navigate(_chatPage);
            UpdateTitle("Chat");
        }
        
        private void UpdateTitle(string pageTitle)
        {
            this.Title = $"{pageTitle} - Anti-Swearing Chat Box";
        }
    }
} 