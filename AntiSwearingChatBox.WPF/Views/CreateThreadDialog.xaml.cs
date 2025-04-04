using System.Collections.Generic;
using System.Windows;
using AntiSwearingChatBox.WPF.Models.Api;
using System.Linq;

namespace AntiSwearingChatBox.WPF.Views
{
    public partial class CreateThreadDialog : Window
    {
        public string ThreadTitle => TitleTextBox.Text;
        public UserModel? SelectedUser => UsersListBox.SelectedItem as UserModel;
        public new bool? DialogResult { get; private set; }

        public CreateThreadDialog(IEnumerable<UserModel> users)
        {
            InitializeComponent();
            UsersListBox.ItemsSource = users;
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ThreadTitle))
            {
                MessageBox.Show("Please enter a thread title.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedUser == null)
            {
                MessageBox.Show("Please select a user.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 