using System;
using System.Globalization;
using System.Windows.Data;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class BoolToColumnConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return 0;

            bool boolValue = (bool)value;
            
            // Default columns if no parameter is provided
            int falseColumn = 0;
            int trueColumn = 2;
            
            // Parse parameter if provided in format "falseColumn:trueColumn"
            if (parameter is string paramStr && !string.IsNullOrEmpty(paramStr) && paramStr.Contains(':'))
            {
                string[] parts = paramStr.Split(':');
                if (parts.Length == 2 && int.TryParse(parts[0], out int falseVal) && int.TryParse(parts[1], out int trueVal))
                {
                    falseColumn = falseVal;
                    trueColumn = trueVal;
                }
            }
            
            return boolValue ? trueColumn : falseColumn;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 