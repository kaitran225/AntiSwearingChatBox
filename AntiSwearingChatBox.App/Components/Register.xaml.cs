using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.App.Views;

namespace AntiSwearingChatBox.App.Components
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Register : UserControl
    {
        public Register()
        {
            InitializeComponent();
            btnLogin.MouseDown += BtnLogin_Click;
        }
        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add proper authentication logic

            // For now, just open the main window to showcase the UI
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Show();

            Window parentWindow = Window.GetWindow(this); // Get the parent window
            parentWindow?.Close();
        }
    }
}
