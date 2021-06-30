namespace Sideways
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(float), typeof(string))]
    public sealed class MillionConverter : IValueConverter
    {
        public static readonly MillionConverter Default = new();

        public static string DisplayText(float value, IFormatProvider? culture = null) => value switch
        {
            > 10_000_000_000 => $"{(value / 1_000_000_000).ToString("F0", culture ?? CultureInfo.CurrentUICulture)}B",
            > 1_000_000_000 => $"{(value / 1_000_000_000).ToString("0.#", culture ?? CultureInfo.CurrentUICulture)}B",
            > 10_000_000 => $"{(value / 1_000_000).ToString("F0", culture ?? CultureInfo.CurrentUICulture)}M",
            > 1_000_000 => $"{(value / 1_000_000).ToString("0.#", culture ?? CultureInfo.CurrentUICulture)}M",
            _ => value.ToString("N0", culture ?? CultureInfo.CurrentUICulture),
        };

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                float f => DisplayText(f, culture),
                null => null,
                _ => throw new InvalidOperationException($"Unknown value {value}"),
            };
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                string { Length: 0 } => null,
                null => null,
                string s
                    when s.EndsWith("M", StringComparison.Ordinal)
                    => float.Parse(s.AsSpan().TrimEnd('M'), NumberStyles.Float, culture) * 1_000_000,
                string s => float.Parse(s.AsSpan(), NumberStyles.Float, culture),
                _ => throw new InvalidOperationException($"Unknown value {value}"),
            };
        }
    }
}
