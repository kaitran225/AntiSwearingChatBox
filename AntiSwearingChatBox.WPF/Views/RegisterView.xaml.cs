using System.Windows;
using System.Windows.Controls;

namespace AntiSwearingChatBox.WPF.Views
{
    public partial class RegisterView : Window
    {

        public RegisterView()
        {
            InitializeComponent();
            //_viewModel = new RegisterViewModel();
            //DataContext = _viewModel;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //if (DataContext is RegisterViewModel viewModel)
            //{
            //    viewModel.Password = ((PasswordBox)sender).Password;
            //}
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            //if (DataContext is RegisterViewModel viewModel)
            //{
            //    viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            //}
        }
    }
} 