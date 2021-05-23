namespace Sideways
{
    using System;
    using Sideways.AlphaVantage;

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

        public static TimeRange FromSlice(Slice slice)
        {
            var end = DateTime.Today.AddDays(-30 * Offset());
            var start = end.AddDays(-30);
            return new TimeRange(start, end);
            int Offset() => slice switch
            {
                Slice.Year1Month1 => 0,
                Slice.Year1Month2 => 1,
                Slice.Year1Month3 => 2,
                Slice.Year1Month4 => 3,
                Slice.Year1Month5 => 4,
                Slice.Year1Month6 => 5,
                Slice.Year1Month7 => 6,
                Slice.Year1Month8 => 7,
                Slice.Year1Month9 => 8,
                Slice.Year1Month10 => 9,
                Slice.Year1Month11 => 10,
                Slice.Year1Month12 => 11,
                Slice.Year2Month1 => 12,
                Slice.Year2Month2 => 13,
                Slice.Year2Month3 => 14,
                Slice.Year2Month4 => 15,
                Slice.Year2Month5 => 16,
                Slice.Year2Month6 => 17,
                Slice.Year2Month7 => 18,
                Slice.Year2Month8 => 19,
                Slice.Year2Month9 => 20,
                Slice.Year2Month10 => 21,
                Slice.Year2Month11 => 22,
                Slice.Year2Month12 => 23,
                _ => throw new ArgumentOutOfRangeException(nameof(slice), slice, null),
            };
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

        public bool Contains(TimeRange other) => this.Contains(other.Min) && this.Contains(other.Max);

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
