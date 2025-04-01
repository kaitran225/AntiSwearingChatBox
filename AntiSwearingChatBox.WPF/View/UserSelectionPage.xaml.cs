using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;


namespace AntiSwearingChatBox.App.Views
{
    public class UserSelectionEventArgs : EventArgs
    {
    }
    
    public partial class UserSelectionPage : Page
    {
        // Standard constructor
        public UserSelectionPage()
        {
            InitializeComponent();
        }
       
        private void CreateGroupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
        }

        private void CreateGroupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void StartChatButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CreateGroupCancelButton_Click(object sender, RoutedEventArgs e)
        {
        }

        private void CreateGroupConfirmButton_Click(object sender, RoutedEventArgs e)
        {
        }
        
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Empty implementation for search functionality
        }
    }
} 