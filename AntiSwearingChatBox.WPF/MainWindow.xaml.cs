using System;
using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.WPF.Services;
using AntiSwearingChatBox.WPF.View;

namespace AntiSwearingChatBox.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LaunchAppButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Open the full application window from the View folder
                var appWindow = new View.MainWindow();
                appWindow.Show();
                
                // Close this starter window
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error launching application: {ex.Message}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 