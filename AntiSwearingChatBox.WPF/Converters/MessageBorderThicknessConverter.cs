using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class MessageBorderThicknessConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new Thickness(0);

            bool isSent = (bool)value;

            return isSent
                ? new Thickness(2) // Increased border thickness for sent messages
                : new Thickness(0); // No border for received messages
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 