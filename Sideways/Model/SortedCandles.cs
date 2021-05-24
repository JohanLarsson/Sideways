namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;

    public readonly struct SortedCandles : IEquatable<SortedCandles>, IReadOnlyList<Candle>
    {
        public static readonly SortedCandles Empty = new(ImmutableArray<Candle>.Empty);

        private readonly ImmutableArray<Candle> candles;

        private SortedCandles(ImmutableArray<Candle> candles)
        {
            this.candles = candles;
        }

        public int Count => this.candles.IsDefault ? 0 : this.candles.Length;

        public Candle this[int index] => this.candles[index];

        public static bool operator ==(SortedCandles left, SortedCandles right) => left.Equals(right);

        public static bool operator !=(SortedCandles left, SortedCandles right) => !left.Equals(right);

        public static SortedCandles Create(params Candle[] candles)
        {
            var builder = new Builder(ImmutableArray.CreateBuilder<Candle>(candles.Length));
            foreach (var candle in candles)
            {
                builder.Add(candle);
            }

            return builder.Create();
        }

        public static Builder CreateBuilder() => new(ImmutableArray.CreateBuilder<Candle>());

        public SortedCandles AdjustBy(SortedSplits splits)
        {
            if (this.candles.IsDefaultOrEmpty ||
                splits.Count == 0)
            {
                return this;
            }

            var splitIndex = splits.Count - 1;
            var c = 1.0;
            var builder = ImmutableArray.CreateBuilder<Candle>(this.candles.Length);
            builder.Count = this.candles.Length;
            for (var i = this.candles.Length - 1; i >= 0; i--)
            {
                var candle = this.candles[i];
                if (splitIndex >= 0 &&
                    candle.Time < splits[splitIndex].Date)
                {
                    c /= splits[splitIndex].Coefficient;
                    splitIndex--;
                }

                builder[i] = candle.Adjust(c);
            }

            return new(builder.MoveToImmutable());
        }

        public int IndexOf(DateTimeOffset end, int startAt)
        {
            if (this.candles.IsDefaultOrEmpty ||
                end < this.candles[0].Time)
            {
                return -1;
            }

            startAt = Math.Clamp(startAt, 0, this.candles.Length);
            if (this.candles[startAt].Time > end)
            {
                for (var i = startAt; i >= 0; i--)
                {
                    if (this.candles[i].Time <= end)
                    {
                        return i;
                    }
                }

                return 0;
            }

            for (var i = startAt; i < this.candles.Length - 1; i++)
            {
                if (this.candles[i + 1].Time > end)
                {
                    return i;
                }
            }

            return this.candles.Length - 1;
        }

        public bool Equals(SortedCandles other) => this.candles.Equals(other.candles);

        public IEnumerator<Candle> GetEnumerator() => ((IEnumerable<Candle>)this.candles).GetEnumerator();

        public override bool Equals(object? obj) => obj is SortedCandles other && this.Equals(other);

        public override int GetHashCode() => this.candles.GetHashCode();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.candles).GetEnumerator();

#pragma warning disable CA1034 // Nested types should not be visible
        public sealed class Builder
#pragma warning restore CA1034 // Nested types should not be visible
        {
            private readonly ImmutableArray<Candle>.Builder inner;

            public Builder(ImmutableArray<Candle>.Builder inner)
            {
                this.inner = inner;
            }

            public void Add(Candle candle)
            {
                Debug.Assert(this.inner.Count == 0 || this.inner[^1].Time < candle.Time, "Must be ascending.");
                this.inner.Add(candle);
            }

            public SortedCandles Create() => new(this.inner.ToImmutable());
        }
    }
}
