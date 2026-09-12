using System;
using System.Globalization;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Talkift.Client.Helpers;

public sealed class ThemeBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string themeKey)
        {
            var isDark = IsSystemDark();
            return themeKey?.ToLower() switch
            {
                "background" => isDark
                    ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 25, 25, 25))
                    : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 255, 255)),
                "foreground" => isDark
                    ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 230, 230, 230))
                    : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 20, 20, 20)),
                "accent" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 0, 120, 215)),
                "card" => isDark
                    ? new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 40, 40, 40))
                    : new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 248, 248, 248)),
                _ => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 128, 128, 128))
            };
        }
        return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 128, 128, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }

    private static bool IsSystemDark()
    {
        try
        {
            var settings = new Windows.UI.ViewManagement.UISettings();
            return settings.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).R < 128;
        }
        catch
        {
            return false;
        }
    }
}

public sealed class StatusColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value is string status)
        {
            return status?.ToLower() switch
            {
                "online" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 50, 205, 50)),
                "away" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 255, 165, 0)),
                "busy" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 220, 20, 60)),
                "offline" => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 128, 128, 128)),
                _ => new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 128, 128, 128))
            };
        }
        return new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(255, 128, 128, 128));
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotSupportedException();
    }
}
