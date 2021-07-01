namespace Sideways
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(DateTime), typeof(DateTimeOffset))]
    [ValueConversion(typeof(DateTimeOffset), typeof(DateTime))]
    public sealed class DateConverter : IValueConverter
    {
        public static readonly DateConverter Default = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                DateTimeOffset d => d.DateTime,
                DateTime d => new DateTimeOffset(d),
                null => null,
                _ => throw new NotSupportedException($"Unknown value: {value}"),
            };
        }

        public object? ConvertBack(object? value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                DateTimeOffset d => d.DateTime,
                DateTime d => new DateTimeOffset(d),
                null => null,
                _ => throw new NotSupportedException($"Unknown value: {value}"),
            };
        }
    }
}
