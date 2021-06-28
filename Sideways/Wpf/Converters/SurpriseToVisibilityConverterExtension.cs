namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ValueConversion(typeof(float), typeof(Visibility))]
    [MarkupExtensionReturnType(typeof(SurpriseToVisibilityConverterExtension))]
    public sealed class SurpriseToVisibilityConverterExtension : MarkupExtension, IValueConverter
    {
        private readonly double minimum;

        public SurpriseToVisibilityConverterExtension(double minimum)
        {
            this.minimum = minimum;
        }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value switch
            {
                double x => Math.Abs(x) > this.minimum ? Visibility.Visible : Visibility.Collapsed,
                float x => Math.Abs(x) > this.minimum ? Visibility.Visible : Visibility.Collapsed,
                int x => Math.Abs(x) > this.minimum ? Visibility.Visible : Visibility.Collapsed,
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
