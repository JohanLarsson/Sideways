namespace Sideways
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(object), typeof(bool))]
    public sealed class CandleIntervalToBoolConverter : IValueConverter
    {
        public static readonly CandleIntervalToBoolConverter Hour = new(CandleInterval.Hour);
        public static readonly CandleIntervalToBoolConverter FifteenMinutes = new(CandleInterval.FifteenMinutes);
        public static readonly CandleIntervalToBoolConverter FiveMinutes = new(CandleInterval.FiveMinutes);
        public static readonly CandleIntervalToBoolConverter Minute = new(CandleInterval.Minute);

        private readonly object trueWhen;

        public CandleIntervalToBoolConverter(CandleInterval trueWhen)
        {
            this.trueWhen = trueWhen;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Equals(value, this.trueWhen);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is true ? this.trueWhen : Binding.DoNothing;
        }
    }
}
