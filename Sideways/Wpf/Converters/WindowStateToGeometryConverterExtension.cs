namespace Sideways
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Markup;
    using System.Windows.Media;

    [ValueConversion(typeof(WindowState), typeof(Geometry))]
    public class WindowStateToGeometryConverterExtension : MarkupExtension, IValueConverter
    {
        public Geometry? MaximizeGeometry { get; set; }

        public Geometry? RestoreGeometry { get; set; }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                WindowState.Maximized => this.RestoreGeometry,
                WindowState.Normal => this.MaximizeGeometry,
                _ => null,
            };
        }

        public override object ProvideValue(IServiceProvider serviceProvider) => this;

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException($"{nameof(WindowStateToGeometryConverterExtension)} can only be used in OneWay bindings");
        }
    }
}
