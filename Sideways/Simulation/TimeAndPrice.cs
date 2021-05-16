namespace Sideways
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct TimeAndPrice : IEquatable<TimeAndPrice>
    {
        public readonly DateTimeOffset Time;
        public readonly float Price;

        public TimeAndPrice(DateTimeOffset time, float price)
        {
            this.Time = time;
            this.Price = price;
        }

        public static bool operator ==(TimeAndPrice left, TimeAndPrice right) => left.Equals(right);

        public static bool operator !=(TimeAndPrice left, TimeAndPrice right) => !left.Equals(right);

        public bool Equals(TimeAndPrice other) => this.Time.Equals(other.Time) && this.Price.Equals(other.Price);

        public override bool Equals(object? obj) => obj is TimeAndPrice other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.Time, this.Price);

        private string GetDebuggerDisplay() => $"{this.Time} {this.Price}";
    }
}
