using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
using AntiSwearingChatBox.WPF.Models.Api;
using AntiSwearingChatBox.WPF.Services.Api;

namespace AntiSwearingChatBox.App.Views
{
    public class UserSelectionEventArgs : EventArgs
    {
        public UserModel? SelectedUser { get; set; }
    }
    
    public partial class UserSelectionPage : Page
    {
        private readonly IApiService _apiService;
        public ObservableCollection<UserModel> Users { get; private set; }
        public ObservableCollection<UserModel> FilteredUsers { get; private set; }
        
        public event EventHandler<UserSelectionEventArgs>? UserSelected;
        
        // Standard constructor
        public UserSelectionPage()
        {
            InitializeComponent();
            
            _apiService = AntiSwearingChatBox.WPF.Services.ServiceProvider.ApiService;
            Users = new ObservableCollection<UserModel>();
            FilteredUsers = new ObservableCollection<UserModel>();
            
            this.DataContext = this;
            this.Loaded += UserSelectionPage_Loaded;
        }
        
        private async void UserSelectionPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadUsers();
        }
        
        private async Task LoadUsers()
        {
            try
            {
                // Clear existing users
                Users.Clear();
                FilteredUsers.Clear();
                
                // Show loading indicator
                LoadingIndicator.Visibility = Visibility.Visible;
                NoUsersMessage.Visibility = Visibility.Collapsed;
                
                // Get users from API
                var users = await _apiService.GetUsersAsync();
                
                // Hide current user from the list
                var currentUser = _apiService.CurrentUser;
                if (currentUser != null)
                {
                    users.RemoveAll(u => u.UserId == currentUser.UserId);
                }
                
                // Update collections
                foreach (var user in users)
                {
                    Users.Add(user);
                    FilteredUsers.Add(user);
                }
                
                // Hide loading indicator
                LoadingIndicator.Visibility = Visibility.Collapsed;
                
                // Show message if no users
                if (Users.Count == 0)
                {
                    NoUsersMessage.Visibility = Visibility.Visible;
                }
                
                Console.WriteLine($"Loaded {Users.Count} users for selection");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
                LoadingIndicator.Visibility = Visibility.Collapsed;
                NoUsersMessage.Visibility = Visibility.Visible;
                NoUsersMessage.Text = "Error loading users. Please try again.";
            }
        }
       
        private void CreateGroupCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SingleUserPanel.Visibility = Visibility.Collapsed;
            GroupUserPanel.Visibility = Visibility.Visible;
        }

        private void CreateGroupCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            SingleUserPanel.Visibility = Visibility.Visible;
            GroupUserPanel.Visibility = Visibility.Collapsed;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService != null)
            {
                this.NavigationService.GoBack();
            }
        }

        private void StartChatButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedUser = UsersListView.SelectedItem as UserModel;
            if (selectedUser != null)
            {
                UserSelected?.Invoke(this, new UserSelectionEventArgs { SelectedUser = selectedUser });
            }
        }

        private void CreateGroupButton_Click(object sender, RoutedEventArgs e)
        {
            // Implementation for group creation
        }

        private void CreateGroupCancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Cancel group creation
        }

        private void CreateGroupConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            // Confirm group creation
        }
        
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchBox.Text.ToLower();
            FilteredUsers.Clear();
            
            if (string.IsNullOrWhiteSpace(searchText))
            {
                foreach (var user in Users)
                {
                    FilteredUsers.Add(user);
                }
            }
            else
            {
                foreach (var user in Users)
                {
                    if (user.Username?.ToLower().Contains(searchText) == true)
                    {
                        FilteredUsers.Add(user);
                    }
                }
            }
            
            // Show message if no matching users
            if (FilteredUsers.Count == 0)
            {
                NoUsersMessage.Visibility = Visibility.Visible;
                NoUsersMessage.Text = "No users match your search.";
            }
            else
            {
                NoUsersMessage.Visibility = Visibility.Collapsed;
            }
        }
    }
} 