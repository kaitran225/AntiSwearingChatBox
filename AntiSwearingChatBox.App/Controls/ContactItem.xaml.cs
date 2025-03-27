using System.Windows.Controls;
using Anti_Swearing_Chat_Box.Core.Models;

namespace Anti_Swearing_Chat_Box.App.Controls
{
    public partial class ContactItem : UserControl
    {
        public static readonly DependencyProperty ContactProperty =
            DependencyProperty.Register("Contact", typeof(Contact), typeof(ContactItem),
                new PropertyMetadata(null, OnContactChanged));

        public Contact Contact
        {
            get => (Contact)GetValue(ContactProperty);
            set => SetValue(ContactProperty, value);
        }

        public ContactItem()
        {
            InitializeComponent();
            DataContext = this;
        }

        private static void OnContactChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ContactItem contactItem && e.NewValue is Contact contact)
            {
                contactItem.UpdateContactInfo(contact);
            }
        }

        private void UpdateContactInfo(Contact contact)
        {
            // Update any UI elements that need to be refreshed when the contact changes
            // This method is called when the Contact property changes
        }
    }
} 