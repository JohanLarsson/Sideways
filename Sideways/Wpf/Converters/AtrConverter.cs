namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Data;

    public sealed class AtrConverter : IMultiValueConverter
    {
        public static readonly AtrConverter Default = new();

        public object? Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values is { Length: > 1 } &&
                values[0] is IReadOnlyList<Candle> { Count: > 21 } candles)
            {
                // https://www.investopedia.com/terms/a/atr.asp
                return TrueRanges().Take(20).Average();

                IEnumerable<double> TrueRanges()
                {
                    for (var i = 0; i < candles.Count - 1; i++)
                    {
                        var h = candles[i].High;
                        var l = candles[i].Low;
                        var cp = candles[i + 1].Close;
                        yield return Math.Max(h - l, Math.Max(Math.Abs(h - cp), Math.Abs(l - cp)));
                    }
                }
            }

            return null;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(AdrConverter)} can only be used in OneWay bindings");
        }
    }
}
