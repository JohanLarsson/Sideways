namespace Sideways
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    [ValueConversion(typeof(object), typeof(bool))]
    public sealed class NullToBooleanConverter : IValueConverter
    {
        public static readonly NullToBooleanConverter TrueWhenNotNull = new(x => x != null);
        public static readonly NullToBooleanConverter TrueWhenNull = new(x => x is null);

        private readonly Func<object, bool> isTrue;

        private NullToBooleanConverter(Func<object, bool> isTrue)
        {
            this.isTrue = isTrue;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return this.isTrue(value);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
