using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using AntiSwearingChatBox.WPF.Components;
using MaterialDesignThemes.Wpf;

namespace AntiSwearingChatBox.WPF.Converters
{
    public class MessageStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is MessageStatus status
                ? status switch
                {
                    MessageStatus.Sent => PackIconKind.CheckCircleOutline,
                    MessageStatus.Delivered => PackIconKind.CheckAll,
                    MessageStatus.Read => PackIconKind.CheckAll,
                    MessageStatus.Failed => PackIconKind.AlertCircleOutline,
                    _ => PackIconKind.CheckCircleOutline
                }
                : PackIconKind.CheckCircleOutline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not MessageStatus status) return new SolidColorBrush(Colors.Gray);

            return status switch
            {
                MessageStatus.Read => new SolidColorBrush(Color.FromRgb(0x47, 0xD0, 0x68)), // Success Green
                MessageStatus.Failed => new SolidColorBrush(Color.FromRgb(0xFF, 0x4D, 0x4D)), // Error Red
                _ => new SolidColorBrush(Color.FromRgb(0xB3, 0xB3, 0xB3)) // Gray
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToOnlineColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool isOnline && isOnline
                ? new SolidColorBrush(Color.FromRgb(0x47, 0xD0, 0x68)) // Success Green
                : new SolidColorBrush(Color.FromRgb(0xB3, 0xB3, 0xB3)); // Gray
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool boolValue && !boolValue
                ? System.Windows.Visibility.Visible
                : System.Windows.Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
} 