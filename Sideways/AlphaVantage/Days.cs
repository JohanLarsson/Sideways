namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;

    public readonly struct Days : IEquatable<Days>
    {
        public readonly ImmutableArray<Candle> Candles;
        public readonly ImmutableArray<Split> Splits;
        public readonly Task<Days>? Download;

        public Days(ImmutableArray<Candle> candles, ImmutableArray<Split> splits, Task<Days>? download)
        {
            this.Candles = candles;
            this.Splits = splits;
            this.Download = download;
        }

        public static bool operator ==(Days left, Days right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Days left, Days right)
        {
            return !left.Equals(right);
        }

        public bool Equals(Days other)
        {
            return this.Candles.Equals(other.Candles) && this.Splits.Equals(other.Splits) && Equals(this.Download, other.Download);
        }

        public override bool Equals(object? obj)
        {
            return obj is Days other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Candles, this.Splits, this.Download);
        }
    }
}
