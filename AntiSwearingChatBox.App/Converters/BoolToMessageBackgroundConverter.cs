using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Anti_Swearing_Chat_Box.App.Converters
{
    public class BoolToMessageBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFromCurrentUser)
            {
                return isFromCurrentUser ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50")) : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525"));
            }
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#252525"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 