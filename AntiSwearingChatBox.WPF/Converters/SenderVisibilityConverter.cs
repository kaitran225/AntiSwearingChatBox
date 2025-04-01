using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class SenderVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || values[0] == null || values[1] == null)
                return Visibility.Visible;

            string messageSender = values[0].ToString()!;
            string currentUser = values[1].ToString()!;

            bool isCurrentUser = string.Equals(messageSender, currentUser, StringComparison.OrdinalIgnoreCase);
            
            // If parameter is "Invert", invert the result
            if (parameter != null && parameter.ToString() == "Invert")
            {
                isCurrentUser = !isCurrentUser;
            }
            
            return isCurrentUser ? Visibility.Visible : Visibility.Collapsed;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 