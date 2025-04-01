using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class MessageBackgroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || values[0] == null || values[1] == null)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f2f5"));

            string messageSender = values[0].ToString();
            string currentUser = values[1].ToString();

            return string.Equals(messageSender, currentUser, StringComparison.OrdinalIgnoreCase)
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0084ff")) // Blue for current user
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f2f5")); // Gray for others
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 