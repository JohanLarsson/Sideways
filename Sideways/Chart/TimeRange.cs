namespace Sideways
{
    using System;

    [System.Diagnostics.DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct TimeRange : IEquatable<TimeRange>
    {
        public readonly DateTimeOffset Min;
        public readonly DateTimeOffset Max;

        public TimeRange(DateTimeOffset min, DateTimeOffset max)
        {
            this.Min = min;
            this.Max = max;
        }

        public static bool operator ==(TimeRange left, TimeRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(TimeRange left, TimeRange right)
        {
            return !left.Equals(right);
        }

        public bool Equals(TimeRange other)
        {
            return this.Min.Equals(other.Min) && this.Max.Equals(other.Max);
        }

        public override bool Equals(object? obj)
        {
            return obj is TimeRange other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Min, this.Max);
        }

        public bool Contains(DateTimeOffset value) => value >= this.Min && value <= this.Max;

        public double Interpolate(DateTimeOffset time)
        {
            var range = this.Max - this.Min;
            if (range.Ticks == 0)
            {
                return 0;
            }

            return (time - this.Min) / range;
        }

        private string GetDebuggerDisplay() => $"Min: {this.Min} Max: {this.Max}";
    }
}
