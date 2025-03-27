using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Anti_Swearing_Chat_Box.App.Converters
{
    public class BoolToHorizontalAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isFromCurrentUser)
            {
                return isFromCurrentUser ? HorizontalAlignment.Right : HorizontalAlignment.Left;
            }
            return HorizontalAlignment.Left;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 