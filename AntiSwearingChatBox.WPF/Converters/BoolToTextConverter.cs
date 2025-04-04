using System;
using System.Globalization;
using System.Windows.Data;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return string.Empty;

            bool boolValue = (bool)value;
            
            // Default text values if no parameter is provided
            string trueText = "True";
            string falseText = "False";
            
            // Parse parameter if provided in format "trueText:falseText"
            if (parameter is string paramStr && !string.IsNullOrEmpty(paramStr) && paramStr.Contains(':'))
            {
                string[] parts = paramStr.Split(':');
                if (parts.Length == 2)
                {
                    trueText = parts[0];
                    falseText = parts[1];
                }
            }
            
            return boolValue ? trueText : falseText;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 