using System.Windows;
using System.Windows.Input;
using MaterialDesignThemes.Wpf;

namespace AntiSwearingChatBox.App.Views
{
    public partial class MainWindow : Window
    {
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

        //private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        //{
        //    if (WindowState == WindowState.Maximized)
        //    {
        //        WindowState = WindowState.Normal;
        //        MaximizeIcon.Kind = PackIconKind.WindowMaximize;
        //    }
        //    else
        //    {
        //        WindowState = WindowState.Maximized;
        //        MaximizeIcon.Kind = PackIconKind.WindowRestore;
        //    }
        //}

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        // Navigation methods
        public void NavigateToLogin()
        {
            MainFrame.Navigate(new LoginPage2());
        }
        
        public void NavigateToRegister()
        {
            MainFrame.Navigate(new RegisterPage2());
        }
        
        public void NavigateToDashboard()
        {
            MainFrame.Navigate(new DashboardPage2());
        }
        
        public void NavigateToChat()
        {
            MainFrame.Navigate(new ChatPage2());
        }
    }
} 