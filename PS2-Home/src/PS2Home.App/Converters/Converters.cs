using Avalonia.Data.Converters;
using Avalonia.Media;
using System;
using System.Globalization;

namespace PS2Home.App.Converters;

/// <summary>
/// Converts a boolean to a brush. Parameter format: "trueBrush|falseBrush" (hex colors or named colors)
/// </summary>
public class BoolToBrushConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2)
            {
                var colorString = boolValue ? parts[0] : parts[1];
                return Brush.Parse(colorString);
            }
        }
        return Brush.Parse("#2A2F3E"); // Default fallback
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Converts a boolean to text. Parameter format: "trueText|falseText"
/// </summary>
public class BoolToTextConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue && parameter is string param)
        {
            var parts = param.Split('|');
            if (parts.Length == 2)
            {
                return boolValue ? parts[0] : parts[1];
            }
        }
        return string.Empty;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}

/// <summary>
/// Subtracts parameter from value (for calculating available apps)
/// </summary>
public class SubtractConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is int val && parameter is int param)
        {
            return val - param;
        }
        return 0;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}