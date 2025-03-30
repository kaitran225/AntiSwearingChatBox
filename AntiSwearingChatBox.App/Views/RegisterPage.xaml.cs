using System.Windows;
using System.Windows.Controls;

namespace AntiSwearingChatBox.App.Views
{
    public partial class RegisterPage : Page
    {

        public RegisterPage()
        {
            InitializeComponent();
        }
        
        private void RegisterComponent_RegisterButtonClicked(object sender, (string username, string email, string password, string confirmPassword) formData)
        {
            // Empty implementation
        }
        
        private void RegisterComponent_LoginRequested(object sender, EventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mainWindow)
            {
                mainWindow.NavigateToLogin();
            }
        }
    }
} 