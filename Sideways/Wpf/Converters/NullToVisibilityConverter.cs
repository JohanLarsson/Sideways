namespace Sideways
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(object), typeof(Visibility))]
    public sealed class NullToVisibilityConverter : IValueConverter
    {
        public static readonly NullToVisibilityConverter VisibleWhenNullElseCollapsed = new(x => x is null ? Visibility.Visible : Visibility.Collapsed);
        public static readonly NullToVisibilityConverter CollapsedWhenNullElseVisible = new(x => x is null ? Visibility.Collapsed : Visibility.Visible);

        private readonly Func<object, Visibility> func;

        private NullToVisibilityConverter(Func<object?, Visibility> func)
        {
            this.func = func;
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return this.func(value);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
