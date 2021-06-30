namespace Sideways
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(int), typeof(int))]
    public sealed class AddOneConverter : IValueConverter
    {
        public static readonly AddOneConverter Default = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                int i => i + 1,
                _ => throw new NotSupportedException("Unknown value"),
            };
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(AddOneConverter)} can only be used in OneWay bindings");
        }
    }
}
