namespace Sideways
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{this.TimeRange} {this.PriceRange}")]
    public readonly struct TimeFloatRange : IEquatable<TimeFloatRange>
    {
        public readonly TimeRange TimeRange;
        public readonly FloatRange PriceRange;

        public TimeFloatRange(TimeRange timeRange, FloatRange priceRange)
        {
            this.TimeRange = timeRange;
            this.PriceRange = priceRange;
        }

        public static bool operator ==(TimeFloatRange left, TimeFloatRange right) => left.Equals(right);

        public static bool operator !=(TimeFloatRange left, TimeFloatRange right) => !left.Equals(right);

        public bool Equals(TimeFloatRange other) => this.TimeRange.Equals(other.TimeRange) && this.PriceRange.Equals(other.PriceRange);

        public override bool Equals(object? obj) => obj is TimeFloatRange other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.TimeRange, this.PriceRange);
    }
}
