namespace Sideways.AlphaVantage
{
    using System;

    public readonly struct DaysAndSplits : IEquatable<DaysAndSplits>
    {
        public readonly DescendingCandles Candles;
        public readonly DescendingSplits Splits;

        public DaysAndSplits(DescendingCandles candles, DescendingSplits splits)
        {
            this.Candles = candles;
            this.Splits = splits;
        }

        public static bool operator ==(DaysAndSplits left, DaysAndSplits right) => left.Equals(right);

        public static bool operator !=(DaysAndSplits left, DaysAndSplits right) => !left.Equals(right);

        public bool Equals(DaysAndSplits other) =>
            this.Candles.Equals(other.Candles) &&
            this.Splits.Equals(other.Splits);

        public override bool Equals(object? obj) => obj is DaysAndSplits other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.Candles, this.Splits);
    }
}
