﻿using Avalonia.Data.Converters;

using System.Globalization;

namespace One.Control.Converters.I18n;

public class I18nKeyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return Get(value);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    public static string? Get(object? value)
    {
        return value switch
        {
            Enum v => $"{value.GetType().Name}_{v}",
            string key => key,
            _ => string.Empty
        };
    }
}