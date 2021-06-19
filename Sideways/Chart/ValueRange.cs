namespace Sideways
{
    using System;
    using System.ComponentModel;

    public readonly struct ValueRange : IEquatable<ValueRange>
    {
        private readonly FloatRange range;
        private readonly Scale scale;

        public ValueRange(FloatRange rawRange, Scale scale)
        {
            this.scale = scale;
            this.range = scale switch
            {
                Scale.Logarithmic => new FloatRange(Log(rawRange.Min), Log(rawRange.Max)),
                Scale.Arithmetic => rawRange,
                _ => throw new InvalidEnumArgumentException(nameof(this.scale), (int)this.scale, typeof(Scale)),
            };
        }

        public static bool operator ==(ValueRange left, ValueRange right) => left.Equals(right);

        public static bool operator !=(ValueRange left, ValueRange right) => !left.Equals(right);

        public bool Equals(ValueRange other) => this.range.Equals(other.range) && this.scale == other.scale;

        public override bool Equals(object? obj) => obj is ValueRange other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.range, (int)this.scale);

        public double Y(float price, double height) => Interpolate.Map(this.range, this.scale == Scale.Logarithmic ? Log(price) : price, new DoubleRange(height, 0));

        public float ValueFromY(double y, double height)
        {
            var raw = Interpolate.Map(new DoubleRange(height, 0), y, this.range);
            return this.scale switch
            {
                Scale.Logarithmic => (float)Math.Exp(raw),
                Scale.Arithmetic => raw,
                _ => throw new InvalidEnumArgumentException(nameof(this.scale), (int)this.scale, typeof(Scale)),
            };
        }

        private static float Log(float value) => (float)(value == 0 ? float.MinValue : Math.Log(value));
    }
}
