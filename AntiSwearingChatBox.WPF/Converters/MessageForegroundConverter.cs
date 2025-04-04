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
                ? new SolidColorBrush(Colors.White) // White text for sent messages (on black background)
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1A2E16")); // Dark green text for received messages
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 