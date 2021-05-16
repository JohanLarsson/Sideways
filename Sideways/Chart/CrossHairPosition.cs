namespace Sideways
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct CrossHairPosition : IEquatable<CrossHairPosition>
    {
        public readonly DateTimeOffset Time;
        public readonly float Price;
        public readonly CandleInterval Interval;

        public CrossHairPosition(DateTimeOffset time, float price, CandleInterval interval)
        {
            this.Time = time;
            this.Price = price;
            this.Interval = interval;
        }

        public static bool operator ==(CrossHairPosition left, CrossHairPosition right) => left.Equals(right);

        public static bool operator !=(CrossHairPosition left, CrossHairPosition right) => !left.Equals(right);

        public bool Equals(CrossHairPosition other) => this.Time.Equals(other.Time) && this.Price.Equals(other.Price) && this.Interval == other.Interval;

        public override bool Equals(object? obj) => obj is CrossHairPosition other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.Time, this.Price, this.Interval);

        private string GetDebuggerDisplay() => $"{this.Time} {this.Price} {this.Interval}";
    }
}
