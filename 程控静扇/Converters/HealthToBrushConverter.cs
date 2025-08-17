using Microsoft.UI;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;
using System;
using 程控静扇.ViewModels;

namespace 程控静扇.Converters;

public class HealthToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is FanHealth health)
        {
            return health switch
            {
                FanHealth.Ok => new SolidColorBrush(Colors.Green),
                FanHealth.Warning => new SolidColorBrush(Colors.Orange),
                FanHealth.Critical => new SolidColorBrush(Colors.Red),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
