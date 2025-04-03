using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class MessageAlignmentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return HorizontalAlignment.Left;

            bool isSent = (bool)value;

            return isSent
                ? HorizontalAlignment.Right // Align sent messages to the right
                : HorizontalAlignment.Left; // Align received messages to the left
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 