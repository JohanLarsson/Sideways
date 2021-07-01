namespace Sideways
{
    using System;
    using System.Windows.Data;
    using System.Windows.Markup;

    [ValueConversion(typeof(double), typeof(double))]
    [MarkupExtensionReturnType(typeof(MultiplyConverterExtension))]
    public sealed class MultiplyConverterExtension : MarkupExtension, IValueConverter
    {
        private readonly double factor;

        public MultiplyConverterExtension(double factor)
        {
            this.factor = factor;
        }

        public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
        {
            return value switch
            {
                double x => x * this.factor,
                float x => x * this.factor,
                int x => x * this.factor,
                null => null,
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
