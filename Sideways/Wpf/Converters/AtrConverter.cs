namespace Sideways
{
    using System;
    using System.Windows.Data;

    public sealed class AtrConverter : IMultiValueConverter
    {
        public static readonly AtrConverter Default = new();

        public object? Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values is { Length: > 1 } &&
                values[0] is DescendingCandles { Count: > 21 } candles)
            {
                // https://www.investopedia.com/terms/a/atr.asp
                return candles[..21].Atr();
            }

            return null;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(AdrConverter)} can only be used in OneWay bindings");
        }
    }
}
