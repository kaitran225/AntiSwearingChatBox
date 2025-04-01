using System;
using System.Globalization;
using System.Windows.Data;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class ReduceWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width && parameter is string paramStr && double.TryParse(paramStr, out double reduction))
            {
                return Math.Max(0, width - reduction);
            }
            
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 