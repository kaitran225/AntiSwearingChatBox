using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.Repository.Models;
using AntiSwearingChatBox.Service.Interface;

namespace AntiSwearingChatBox.App.Views
{
    public partial class UserSelectionPage : Page
    {
        public ObservableCollection<UserViewModel> Users { get; set; } = new ObservableCollection<UserViewModel>();
        private List<UserViewModel> _allUsers = new List<UserViewModel>();
        public User SelectedUser { get; private set; }
        public List<User> SelectedUsers { get; private set; } = new List<User>();
        public bool IsGroupChat { get; private set; } = false;
        public string GroupName { get; private set; } = string.Empty;
        
        // Event to notify when selection is complete
        public event EventHandler<UserSelectionEventArgs> SelectionComplete;
        public event EventHandler SelectionCancelled;

        private readonly IUserService _userService;
        private readonly int _currentUserId;

        public UserSelectionPage(IUserService userService, int currentUserId)
        {
            InitializeComponent();
            
            _userService = userService;
            _currentUserId = currentUserId;
            
            LoadUsers();
            
            // Initial focus on search box
            Loaded += (s, e) => SearchBox.Focus();
        }
        
        private void LoadUsers()
        {
            try
            {
                // Get all users from service
                var allUsers = _userService.GetAll();
                
                // Convert users to view models and exclude current user
                foreach (var user in allUsers.Where(u => u.UserId != _currentUserId))
                {
                    var viewModel = new UserViewModel
                    {
                        UserId = user.UserId,
                        Username = user.Username,
                        Email = user.Email ?? "(No email)",
                        Initials = !string.IsNullOrEmpty(user.Username) ? 
                                  user.Username.Substring(0, Math.Min(2, user.Username.Length)).ToUpper() : "?",
                        User = user
                    };
                    
                    _allUsers.Add(viewModel);
                    Users.Add(viewModel);
                }

                UsersListView.ItemsSource = Users;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading users: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            
            // Set focus to group name textbox
            GroupNameTextBox.Focus();
        }

        private void CreateGroupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            // Hide group name input
            GroupNameGrid.Visibility = Visibility.Collapsed;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Notify that selection was cancelled
            SelectionCancelled?.Invoke(this, EventArgs.Empty);
            
            // Navigate back to chat
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToChat();
            }
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
                // Automatically check group chat option when multiple users selected
                CreateGroupCheckBox.IsChecked = true;
                MessageBox.Show("You've selected multiple users. The 'Create as Group Chat' option has been enabled.", 
                    "Group Chat Required", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            IsGroupChat = CreateGroupCheckBox.IsChecked.GetValueOrDefault();
            
            if (IsGroupChat && GroupNameGrid.Visibility == Visibility.Visible && string.IsNullOrWhiteSpace(GroupNameTextBox.Text))
            {
                MessageBox.Show("Please enter a name for the group chat.", "Group Name Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                GroupNameTextBox.Focus();
                return;
            }

            // For single user chat
            if (!IsGroupChat && selectedItems.Count == 1)
            {
                SelectedUser = selectedItems[0].User;
                
                // Notify selection complete
                SelectionComplete?.Invoke(this, new UserSelectionEventArgs { 
                    SelectedUser = SelectedUser,
                    SelectedUsers = null,
                    IsGroupChat = false
                });
            }
            // For group chat
            else if (IsGroupChat)
            {
                // Get selected users
                SelectedUsers = selectedItems.Select(vm => vm.User).ToList();
                GroupName = GroupNameTextBox.Text;
                
                // Notify selection complete
                SelectionComplete?.Invoke(this, new UserSelectionEventArgs { 
                    SelectedUser = null,
                    SelectedUsers = SelectedUsers,
                    IsGroupChat = true,
                    GroupName = GroupName
                });
            }
            
            // Navigate back to chat
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToChat();
            }
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
                GroupNameTextBox.Focus();
                return;
            }

            // Set properties
            IsGroupChat = true;
            GroupName = GroupNameTextBox.Text;
            SelectedUsers = selectedItems.Select(vm => vm.User).ToList();
            
            // Notify selection complete
            SelectionComplete?.Invoke(this, new UserSelectionEventArgs { 
                SelectedUser = null,
                SelectedUsers = SelectedUsers,
                IsGroupChat = true,
                GroupName = GroupName
            });
            
            // Navigate back to chat
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToChat();
            }
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
    
    public class UserSelectionEventArgs : EventArgs
    {
        public User SelectedUser { get; set; }
        public List<User> SelectedUsers { get; set; }
        public bool IsGroupChat { get; set; }
        public string GroupName { get; set; }
    }
} 