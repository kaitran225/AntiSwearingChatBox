using System;
using System.Windows;
using System.Windows.Controls;

namespace AntiSwearingChatBox.App.Views
{
    /// <summary>
    /// Interaction logic for DashboardPage2.xaml
    /// </summary>
    public partial class DashboardPage2 : Page
    {
        public DashboardPage2()
        {
            InitializeComponent();
            
            // You could load dynamic data here if needed
            LoadDashboardData();
        }
        
        private void LoadDashboardData()
        {
            // You could load real data from services here
            // For now, we're using static data defined in XAML
        }

        private void GoToChat_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to chat
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToChat();
            }
        }

        private void ManageProfile_Click(object sender, RoutedEventArgs e)
        {
            // This would navigate to a profile management page in a real app
            MessageBox.Show("Profile management is not implemented yet.", "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ViewReports_Click(object sender, RoutedEventArgs e)
        {
            // This would navigate to a reports page in a real app
            MessageBox.Show("Detailed reports are not implemented yet.", "Not Implemented", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
} 