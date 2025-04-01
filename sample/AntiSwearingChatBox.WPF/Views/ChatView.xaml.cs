using System.Windows;
using AntiSwearingChatBox.WPF.ViewModels;

namespace AntiSwearingChatBox.WPF.Views
{
    public partial class ChatView : Window
    {
        public ChatView(string username)
        {
            InitializeComponent();
            DataContext = new ChatViewModel(username);
        }
    }
} 