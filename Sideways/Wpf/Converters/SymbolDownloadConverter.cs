namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Windows.Data;
    using Sideways.AlphaVantage;

    [ValueConversion(typeof(object[]), typeof(SymbolDownloads))]
    public sealed class SymbolDownloadConverter : IMultiValueConverter
    {
        public static readonly SymbolDownloadConverter Default = new();

        public object? Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values is { Length: 2 })
            {
                return ((ImmutableSortedSet<SymbolDownloads>)values[1]).SingleOrDefault(x => x.Symbol == (string)values[0]);
            }

            return null;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(SymbolDownloadConverter)} can only be used in OneWay bindings");
        }
    }
}
