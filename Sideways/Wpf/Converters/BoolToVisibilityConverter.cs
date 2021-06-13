namespace Sideways
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public sealed class BoolToVisibilityConverter : IValueConverter
    {
        public static readonly BoolToVisibilityConverter VisibleWhenTrueElseCollapsed = new(Visibility.Visible, Visibility.Collapsed);

        private readonly object whenFalse;
        private readonly object whenTrue;

        private BoolToVisibilityConverter(Visibility whenTrue, Visibility whenFalse)
        {
            this.whenTrue = whenTrue;
            this.whenFalse = whenFalse;
        }

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value switch
            {
                true => this.whenTrue,
                false => this.whenFalse,
                _ => Visibility.Visible,
            };
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
