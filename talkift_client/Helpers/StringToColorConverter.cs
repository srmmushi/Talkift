using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Talkift.Client.Helpers;

public sealed class StringToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string colorString)
        {
            try
            {
                if (colorString.StartsWith("#") && colorString.Length == 7)
                {
                    return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255,
                        System.Convert.ToByte(colorString.Substring(1, 2), 16),
                        System.Convert.ToByte(colorString.Substring(3, 2), 16),
                        System.Convert.ToByte(colorString.Substring(5, 2), 16)));
                }

                if (colorString.StartsWith("#") && colorString.Length == 9)
                {
                    return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(
                        System.Convert.ToByte(colorString.Substring(7, 2), 16),
                        System.Convert.ToByte(colorString.Substring(1, 2), 16),
                        System.Convert.ToByte(colorString.Substring(3, 2), 16),
                        System.Convert.ToByte(colorString.Substring(5, 2), 16)));
                }

                return new SolidColorBrush(Microsoft.UI.Colors.Gray);
            }
            catch
            {
                return new SolidColorBrush(Microsoft.UI.Colors.Gray);
            }
        }
        return new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color.ToString();
        }
        return string.Empty;
    }
}

public sealed class IntToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is int intValue)
        {
            var r = (byte)((intValue >> 16) & 0xFF);
            var g = (byte)((intValue >> 8) & 0xFF);
            var b = (byte)(intValue & 0xFF);
            return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, r, g, b));
        }
        return new SolidColorBrush(Microsoft.UI.Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
