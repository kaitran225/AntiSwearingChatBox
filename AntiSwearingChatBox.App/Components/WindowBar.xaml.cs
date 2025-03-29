using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace AntiSwearingChatBox.App.Components
{
    /// <summary>
    /// Interaction logic for WindowBar.xaml
    /// </summary>
    public partial class WindowBar : UserControl
    {
        public WindowBar()
        {
            InitializeComponent();
            btnClose.Click += BtnClose_Click;
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // Close the application
            Application.Current.Shutdown();
        }

    }
}
