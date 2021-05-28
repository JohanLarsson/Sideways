namespace Sideways.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Data;

    public sealed class AverageDollarVolumeConverter : IMultiValueConverter
    {
        public static readonly AverageDollarVolumeConverter Default = new();

        public object? Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values is { Length: > 1 } &&
                values[0] is IReadOnlyList<Candle> { Count: > 20 } candles)
            {
                return candles.Average(x => x.Close * x.Volume) / 1_000_000;
            }

            return null;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(AdrConverter)} can only be used in OneWay bindings");
        }
    }
}
