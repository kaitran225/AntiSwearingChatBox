using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class MessageAlignmentConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2 || values[0] == null || values[1] == null)
                return HorizontalAlignment.Left;

            string messageSender = values[0].ToString();
            string currentUser = values[1].ToString();

            return string.Equals(messageSender, currentUser, StringComparison.OrdinalIgnoreCase) 
                ? HorizontalAlignment.Right 
                : HorizontalAlignment.Left;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 