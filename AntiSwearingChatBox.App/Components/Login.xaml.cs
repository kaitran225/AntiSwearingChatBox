using System.Windows;
using System.Windows.Controls;
using AntiSwearingChatBox.App.Views;

namespace AntiSwearingChatBox.App.Components
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : UserControl
    {
        public Login()
        {
            InitializeComponent();
            btnLogin.Click += BtnLogin_Click;
            btnRegister.MouseDown += BtnRegister_Click;
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Add proper authentication logic

            // For now, just open the main window to showcase the UI
            ChatWindow chatWindow = new ChatWindow();
            chatWindow.Show();

            // Close the login window
            Window parentWindow = Window.GetWindow(this); // Get the parent window
            parentWindow?.Close();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            // Open register window
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();

            // Close the login window
            Window parentWindow = Window.GetWindow(this); // Get the parent window
            parentWindow?.Close();
        }
    }
}
