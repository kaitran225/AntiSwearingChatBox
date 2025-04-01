using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class MessageForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Colors.Black);

            bool isSent = (bool)value;

            return isSent
                ? new SolidColorBrush(Colors.White) // White text for sent messages
                : new SolidColorBrush(Colors.Black); // Black text for received messages
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 