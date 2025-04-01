using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class BoolToColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSent)
            {
                return isSent ? 2 : 0; // 0 for received (left), 2 for sent (right)
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 