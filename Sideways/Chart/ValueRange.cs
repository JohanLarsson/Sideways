namespace Sideways
{
    using System;

    public readonly struct ValueRange
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
            };
        }

        public double Y(float price, double height) => Interpolate.Map(this.range, this.scale == Scale.Logarithmic ? Log(price) : price, new DoubleRange(height, 0));

        public float ValueFromY(double y, double height)
        {
            var raw = Interpolate.Map(new DoubleRange(height, 0), y, this.range);
            return this.scale switch
            {
                Scale.Logarithmic => (float)Math.Exp(raw),
                Scale.Arithmetic => raw,
            };
        }

        private static float Log(float value) => (float)(value == 0 ? float.MinValue : Math.Log(value));
    }
}
