namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    public class PercentTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string))
            {
                return true;
            }

            return base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            return value switch
            {
                string { Length: 0 } => null,
                string s => Percent.Parse(s, culture),
                _ => base.ConvertFrom(context, culture, value),
            };
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object? ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value is Percent percent)
            {
                if (destinationType == typeof(string))
                {
                    return percent.ToString("R", culture);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
