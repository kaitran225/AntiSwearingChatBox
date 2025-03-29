using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.Repository.Models;

namespace AntiSwearingChatBox.App.Views
{
    public partial class UserSelectionDialog : Window
    {
        public ObservableCollection<UserViewModel> Users { get; set; } = new ObservableCollection<UserViewModel>();
        private List<UserViewModel> _allUsers = new List<UserViewModel>();
        public List<User> SelectedUsers { get; private set; } = new List<User>();
        public bool IsGroupChat { get; private set; } = false;
        public string GroupName { get; private set; } = string.Empty;

        public UserSelectionDialog(List<User> users, int currentUserId)
        {
            InitializeComponent();

            // Convert users to view models and exclude current user
            foreach (var user in users.Where(u => u.UserId != currentUserId))
            {
                var viewModel = new UserViewModel
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email ?? "(No email)",
                    Initials = !string.IsNullOrEmpty(user.Username) ? user.Username.Substring(0, 1).ToUpper() : "?",
                    User = user
                };
                
                _allUsers.Add(viewModel);
                Users.Add(viewModel);
            }

            UsersListView.ItemsSource = Users;
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            Users.Clear();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var user in _allUsers)
                {
                    Users.Add(user);
                }
            }
            else
            {
                foreach (var user in _allUsers.Where(u => 
                    u.Username.ToLower().Contains(searchText) || 
                    (u.Email != null && u.Email.ToLower().Contains(searchText))))
                {
                    Users.Add(user);
                }
            }
        }

        private void CreateGroupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            // Show group name input
            GroupNameGrid.Visibility = Visibility.Visible;
            Height += 50;
        }

        private void CreateGroupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Hide group name input
            GroupNameGrid.Visibility = Visibility.Collapsed;
            Height -= 50;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void StartChatButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = UsersListView.SelectedItems.Cast<UserViewModel>().ToList();
            
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one user to chat with.", "No Users Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (selectedItems.Count > 1 && !CreateGroupCheckBox.IsChecked.GetValueOrDefault())
            {
                MessageBox.Show("You've selected multiple users. Please check 'Create as Group Chat' to create a group chat.", 
                    "Multiple Users Selected", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsGroupChat = CreateGroupCheckBox.IsChecked.GetValueOrDefault();
            
            if (IsGroupChat && GroupNameGrid.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(GroupNameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for the group chat.", "Group Name Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Get selected users
            SelectedUsers = selectedItems.Select(vm => vm.User).ToList();
            
            DialogResult = true;
            Close();
        }

        private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = UsersListView.SelectedItems.Cast<UserViewModel>().ToList();
            
            if (selectedItems.Count == 0)
            {
                MessageBox.Show("Please select at least one user for the group chat.", "No Users Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(GroupNameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for the group chat.", "Group Name Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Set properties
            IsGroupChat = true;
            GroupName = GroupNameTextBox.Text;
            SelectedUsers = selectedItems.Select(vm => vm.User).ToList();
            
            DialogResult = true;
            Close();
        }
    }

    public class UserViewModel
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Initials { get; set; }
        public User User { get; set; }
    }
} 