// ReSharper disable CompareOfFloatsByEqualityOperator
namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    [TypeConverter(typeof(PercentTypeConverter))]
    public readonly struct Percent : IEquatable<Percent>, IComparable<Percent>, IComparable, IFormattable
    {
        public static readonly Percent MinValue = new(float.MinValue);
        public static readonly Percent MaxValue = new(float.MaxValue);

        private readonly float value;

        public Percent(float value)
        {
            this.value = value;
        }

        public static bool operator ==(Percent left, Percent right) => left.value == right.value;

        public static bool operator !=(Percent left, Percent right) => left.value != right.value;

        public static bool operator <(Percent left, Percent right) => left.value < right.value;

        public static bool operator >(Percent left, Percent right) => left.value > right.value;

        public static bool operator <=(Percent left, Percent right) => left.value <= right.value;

        public static bool operator >=(Percent left, Percent right) => left.value >= right.value;

        public static Percent Change(float @from, float to) => new(100 * (to - @from) / Math.Abs(@from));

        public static Percent Parse(string text, IFormatProvider? formatProvider) => new(float.Parse(text.AsSpan().TrimEnd('%'), NumberStyles.Float, formatProvider));

        public bool IsBetween(Percent min, Percent max) => this >= min && this <= max;

        public override string ToString() => $"{this.value}%";

        public string ToString(string? format, IFormatProvider? formatProvider)
        {
            return format switch
            {
                "S0" => this.value.ToString("F0", formatProvider),
                "S1" => this.value.ToString("F1", formatProvider),
                _ => $"{this.value.ToString(format, formatProvider)}%",
            };
        }

        public bool Equals(Percent other) => this.value == other.value;

        public override bool Equals(object? obj) => obj is Percent other && this.Equals(other);

        public override int GetHashCode() => this.value.GetHashCode();

        public int CompareTo(Percent other) => this.value.CompareTo(other.value);

        public int CompareTo(object? obj)
        {
            if (obj is null)
            {
                return 1;
            }

            return obj is Percent other ? this.CompareTo(other) : throw new ArgumentException($"Object must be of type {nameof(Percent)}");
        }
    }
}
