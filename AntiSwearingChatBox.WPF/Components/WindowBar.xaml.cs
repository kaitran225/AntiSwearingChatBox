using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AntiSwearingChatBox.WPF.Components
{
    /// <summary>
    /// Interaction logic for WindowBar.xaml
    /// </summary>
    public partial class WindowBar : UserControl
    {
        public WindowBar()
        {
            InitializeComponent();
            
            // Register event handlers
            btnMinimize.Click += BtnMinimize_Click;
            btnMaximize.Click += BtnMaximize_Click;
            btnClose.Click += BtnClose_Click;
        }
        
        /// <summary>
        /// Updates the window title text
        /// </summary>
        /// <param name="title">The new title to display</param>
        public void SetWindowTitle(string title)
        {
            WindowTitle.Text = $"Anti-Swearing Chat - {title}";
        }
        
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    Window window = Window.GetWindow(this);
                    if (window != null)
                    {
                        window.DragMove();
                        e.Handled = true; // Mark as handled to prevent bubbling
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error dragging window from title bar: {ex.Message}");
                }
            }
        }
        
        private void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized;
            }
        }
        
        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    window.WindowState = WindowState.Normal;
                }
                else
                {
                    window.WindowState = WindowState.Maximized;
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.Close();
            }
        }
    }
}
