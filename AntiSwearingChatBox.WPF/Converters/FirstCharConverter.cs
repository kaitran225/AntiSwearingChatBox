using System;
using System.Globalization;
using System.Windows.Data;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class FirstCharConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return "?";

            string text = value.ToString()!;
            if (string.IsNullOrWhiteSpace(text))
                return "?";

            // Get the first character, ensuring it's a letter if possible
            foreach (char c in text.Trim())
            {
                if (char.IsLetter(c))
                {
                    return c.ToString().ToUpper();
                }
            }

            // Fallback to first character if no letter found
            return text[0].ToString().ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 