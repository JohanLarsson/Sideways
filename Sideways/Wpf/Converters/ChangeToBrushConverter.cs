namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Windows.Data;
    using System.Windows.Media;

    [ValueConversion(typeof(ImmutableArray<Percent>), typeof(SolidColorBrush))]
    public sealed class ChangeToBrushConverter : IValueConverter
    {
        public static readonly ChangeToBrushConverter Item1 = new(0);
        public static readonly ChangeToBrushConverter Item2 = new(1);
        public static readonly ChangeToBrushConverter Item3 = new(2);
        public static readonly ChangeToBrushConverter Item4 = new(3);
        public static readonly ChangeToBrushConverter Item5 = new(4);
        public static readonly ChangeToBrushConverter Item6 = new(5);
        public static readonly ChangeToBrushConverter Item7 = new(6);
        public static readonly ChangeToBrushConverter Item8 = new(7);

        private readonly int index;

        public ChangeToBrushConverter(int index)
        {
            this.index = index;
        }

        public static SolidColorBrush Brush(Percent change)
        {
            if (change <= new Percent(-25))
            {
                return Brushes.Decreasing;
            }

            if (change >= new Percent(25))
            {
                return Brushes.Increasing;
            }

            return Brushes.Gray;
        }

        public object? Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is ImmutableArray<Percent> changes &&
                this.index < changes.Length)
            {
               return Brush(changes[this.index]);
            }

            return null;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(ChangeToBrushConverter)} can only be used in OneWay bindings");
        }
    }
}
