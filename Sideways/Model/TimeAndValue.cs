namespace Sideways
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct TimeAndValue : IEquatable<TimeAndValue>
    {
        public readonly DateTimeOffset Time;
        public readonly float Price;

        public TimeAndValue(DateTimeOffset time, float price)
        {
            this.Time = time;
            this.Price = price;
        }

        public static bool operator ==(TimeAndValue left, TimeAndValue right) => left.Equals(right);

        public static bool operator !=(TimeAndValue left, TimeAndValue right) => !left.Equals(right);

        public bool Equals(TimeAndValue other) => this.Time.Equals(other.Time) && this.Price.Equals(other.Price);

        public override bool Equals(object? obj) => obj is TimeAndValue other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.Time, this.Price);

        private string GetDebuggerDisplay() => $"{this.Time} {this.Price}";
    }
}
