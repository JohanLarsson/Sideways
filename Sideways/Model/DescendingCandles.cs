namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;

    public readonly struct DescendingCandles : IEquatable<DescendingCandles>, IReadOnlyList<Candle>
    {
        private readonly ImmutableArray<Candle> candles;

        private DescendingCandles(ImmutableArray<Candle> candles)
        {
            this.candles = candles;
        }

        public int Count => this.candles.Length;

        public Candle this[int index] => this.candles[index];

        public static bool operator ==(DescendingCandles left, DescendingCandles right) => left.Equals(right);

        public static bool operator !=(DescendingCandles left, DescendingCandles right) => !left.Equals(right);

        public static Builder CreateBuilder() => new();

        public DescendingCandles AdjustBy(DescendingSplits splits)
        {
            var splitIndex = 0;
            var c = 1.0;
            var builder = ImmutableArray.CreateBuilder<Candle>(this.candles.Length);
            foreach (var candle in this.candles)
            {
                if (splitIndex < splits.Count &&
                    candle.Time < splits[splitIndex].Date)
                {
                    c /= splits[splitIndex].Coefficient;
                    splitIndex++;
                }

                builder.Add(candle.Adjust(c));
            }

            return new(builder.MoveToImmutable());
        }

        public bool Equals(DescendingCandles other) => this.candles.Equals(other.candles);

        public IEnumerator<Candle> GetEnumerator() => ((IEnumerable<Candle>)this.candles).GetEnumerator();

        public override bool Equals(object? obj) => obj is DescendingCandles other && this.Equals(other);

        public override int GetHashCode() => this.candles.GetHashCode();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.candles).GetEnumerator();

#pragma warning disable CA1034 // Nested types should not be visible
        public sealed class Builder
#pragma warning restore CA1034 // Nested types should not be visible
        {
            private readonly ImmutableArray<Candle>.Builder inner = ImmutableArray.CreateBuilder<Candle>();

            public void Add(Candle candle)
            {
                Debug.Assert(this.inner.Count == 0 || this.inner.Last().Time > candle.Time, "this.inner.Count == 0 || this.inner.Last().Time> candle.Time");
                this.inner.Add(candle);
            }

            public DescendingCandles Create() => new(this.inner.ToImmutable());
        }
    }
}
