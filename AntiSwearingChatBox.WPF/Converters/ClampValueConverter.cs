using System;
using System.Globalization;
using System.Windows.Data;

namespace AntiSwearingChatBox.WPF.Converters
{
    /// <summary>
    /// Clamps a value to a maximum (default 5) for proper display in progress bars and indicators
    /// </summary>
    public class ClampValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                // Default max value is 5
                int maxValue = 5;
                
                // If parameter is provided, try to parse it as the max value
                if (parameter is string paramStr && int.TryParse(paramStr, out int parsedMax))
                {
                    maxValue = parsedMax;
                }
                
                // Clamp the value to the max
                return Math.Min(intValue, maxValue);
            }
            
            // Return the original value if it's not an integer
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 