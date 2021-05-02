namespace Sideways
{
    using System;

    [System.Diagnostics.DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct DoubleRange : IEquatable<DoubleRange>
    {
        public readonly double Min;
        public readonly double Max;

        public DoubleRange(double min, double max)
        {
            this.Min = min;
            this.Max = max;
        }

        public static bool operator ==(DoubleRange left, DoubleRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DoubleRange left, DoubleRange right)
        {
            return !left.Equals(right);
        }

        public bool Equals(DoubleRange other)
        {
            return this.Min.Equals(other.Min) && this.Max.Equals(other.Max);
        }

        public override bool Equals(object? obj)
        {
            return obj is DoubleRange other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Min, this.Max);
        }

        public bool Contains(double value) => value >= this.Min && value <= this.Max;

        public double Interpolate(double value)
        {
            var range = this.Max - this.Min;
            if (range == 0)
            {
                return 0;
            }

            return (value - this.Min) / range;
        }

        private string GetDebuggerDisplay() => $"Min: {this.Min} Max: {this.Max}";
    }
}
