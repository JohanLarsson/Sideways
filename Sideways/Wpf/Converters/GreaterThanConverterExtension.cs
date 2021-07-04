namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ValueConversion(typeof(float), typeof(bool))]
    [MarkupExtensionReturnType(typeof(GreaterThanConverterExtension))]
    public sealed class GreaterThanConverterExtension : MarkupExtension, IValueConverter
    {
        private readonly double minimum;

        public GreaterThanConverterExtension(double minimum)
        {
            this.minimum = minimum;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value switch
            {
                double x => x > this.minimum,
                float x => x > this.minimum,
                int x => x > this.minimum,
                Percent x => x.Scalar > this.minimum,
                null => Visibility.Collapsed,
                _ => throw new NotSupportedException($"Not handling value of type {value.GetType().Name}"),
            };
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(SurpriseToVisibilityConverterExtension)} can only be used in OneWay bindings");
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;
    }
}
