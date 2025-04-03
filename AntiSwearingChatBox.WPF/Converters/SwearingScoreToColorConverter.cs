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
                else if (score <= 2)
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0xC1, 0x07)); // Amber - mild swearing
                else if (score <= 4)
                    return new SolidColorBrush(Color.FromRgb(0xFF, 0x8C, 0x00)); // Orange - moderate swearing
                else
                    return new SolidColorBrush(Color.FromRgb(0xF4, 0x43, 0x36)); // Red - severe swearing
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