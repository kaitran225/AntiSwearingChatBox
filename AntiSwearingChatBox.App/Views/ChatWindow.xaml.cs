using System;
using System.Windows;
using System.Windows.Input;

namespace AntiSwearingChatBox.App.Views
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public ChatWindow()
        {
            InitializeComponent();
            
            // Attach event handlers for admin dashboard and chat menu
            var adminButton = this.FindName("AdminDashboardButton") as System.Windows.Controls.Button;
            if (adminButton != null)
            {
                adminButton.Click += AdminDashboardButton_Click;
            }
            
            var closeAdminButton = this.FindName("CloseAdminDashboardButton") as System.Windows.Controls.Button;
            if (closeAdminButton != null)
            {
                closeAdminButton.Click += CloseAdminDashboardButton_Click;
            }
            
            var chatMenuButton = this.FindName("ChatMenuButton") as System.Windows.Controls.Button;
            if (chatMenuButton != null)
            {
                chatMenuButton.Click += ChatMenuButton_Click;
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        private void AdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle admin dashboard visibility
            var adminPanel = this.FindName("AdminDashboardPanel") as System.Windows.Controls.Grid;
            if (adminPanel != null)
            {
                adminPanel.Visibility = Visibility.Visible;
            }
        }
        
        private void CloseAdminDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide admin dashboard
            var adminPanel = this.FindName("AdminDashboardPanel") as System.Windows.Controls.Grid;
            if (adminPanel != null)
            {
                adminPanel.Visibility = Visibility.Collapsed;
            }
        }
        
        private void ChatMenuButton_Click(object sender, RoutedEventArgs e)
        {
            // Toggle chat menu popup
            var chatMenuPopup = this.FindName("ChatMenuPopup") as System.Windows.Controls.Primitives.Popup;
            if (chatMenuPopup != null)
            {
                chatMenuPopup.IsOpen = !chatMenuPopup.IsOpen;
            }
        }
    }
} 