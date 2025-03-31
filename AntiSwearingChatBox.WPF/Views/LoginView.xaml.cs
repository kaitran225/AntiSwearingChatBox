using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.WPF.ViewModels;

namespace AntiSwearingChatBox.WPF.Views
{
    public partial class LoginView : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginView()
        {
            InitializeComponent();
            _viewModel = new LoginViewModel();
            DataContext = _viewModel;
            
            // Set initial focus on the username textbox
            Loaded += (s, e) => 
            {
                if (string.IsNullOrEmpty(_viewModel.Username))
                {
                    var firstChild = this.FindName("UsernameTextBox") as UIElement;
                    firstChild?.Focus();
                }
            };
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }
    }
} 