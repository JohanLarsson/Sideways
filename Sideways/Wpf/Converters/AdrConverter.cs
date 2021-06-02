namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Data;

    public sealed class AdrConverter : IMultiValueConverter
    {
        public static readonly AdrConverter Default = new();

        public object? Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values is { Length: > 1 } &&
                values[0] is IReadOnlyList<Candle> { Count: > 20 } candles)
            {
                return 100 * (candles.Take(20).Average(x => x.High / x.Low) - 1);
            }

            return null;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(AdrConverter)} can only be used in OneWay bindings");
        }
    }
}
