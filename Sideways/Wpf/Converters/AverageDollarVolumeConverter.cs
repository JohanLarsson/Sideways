namespace Sideways
{
    using System;
    using System.Windows.Data;

    public sealed class AverageDollarVolumeConverter : IMultiValueConverter
    {
        public static readonly AverageDollarVolumeConverter Default = new();

        public object? Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values is { Length: > 1 } &&
                values[0] is DescendingCandles { Count: > 20 } candles)
            {
                return MillionConverter.DisplayText(candles.AsSpan()[..20].Average(x => x.Close * x.Volume), culture);
            }

            return null;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(AdrConverter)} can only be used in OneWay bindings");
        }
    }
}
