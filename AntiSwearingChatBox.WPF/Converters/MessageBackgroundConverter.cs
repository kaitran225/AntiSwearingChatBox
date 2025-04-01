using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class MessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f2f5"));

            bool isSent = (bool)value;

            return isSent
                ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0084ff")) // Blue for sent messages
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#f0f2f5")); // Gray for received messages
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 