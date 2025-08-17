using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.UI;
using 程控静扇.Services; // For IpmiOperationStatus

namespace 程控静扇.Converters
{
    public class IpmiStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is IpmiOperationStatus status)
            {
                return status switch
                {
                    IpmiOperationStatus.Success => new SolidColorBrush(Color.FromArgb(255, 0x66, 0xCC, 0xFF)),
                    IpmiOperationStatus.Failure => new SolidColorBrush(Color.FromArgb(255, 0xEE, 0x00, 0x00)),
                    _ => new SolidColorBrush(Microsoft.UI.Colors.Gray), // Default or any other state
                };
            }
            return new SolidColorBrush(Microsoft.UI.Colors.Gray); // Default if value is not IpmiOperationStatus
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}