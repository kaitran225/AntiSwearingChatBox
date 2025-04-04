using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class SwearingScoreToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int score)
            {
                if (score == 0)
                    return new SolidColorBrush(Color.FromRgb(0x47, 0xD0, 0x68)); // Green - no swearing
                else if (score == 1)
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0xD5, 0x4F)); // Light Amber - very mild swearing
                else if (score == 2)
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07)); // Amber - mild swearing
                else if (score == 3)
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0x98, 0x00)); // Dark Amber - moderate swearing
                else if (score == 4)
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0x57, 0x22)); // Orange-Red - severe swearing
                else
                    return new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)); // Red - critical swearing level
            }
            
            // Default case
            return new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)); // Gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 