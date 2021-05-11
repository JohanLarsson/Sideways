namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;

    public readonly struct DescendingDays : IEquatable<DescendingDays>, IReadOnlyList<Candle>
    {
        private readonly ImmutableArray<Candle> candles;

        public DescendingDays(ImmutableArray<Candle> candles)
        {
            this.candles = candles;
        }

        public int Count => this.candles.Length;

        public Candle this[int index] => this.candles[index];

        public static bool operator ==(DescendingDays left, DescendingDays right) => left.Equals(right);

        public static bool operator !=(DescendingDays left, DescendingDays right) => !left.Equals(right);

        public static Builder CreateBuilder() => new();

        public bool Equals(DescendingDays other) => this.candles.Equals(other.candles);

        public IEnumerator<Candle> GetEnumerator() => ((IEnumerable<Candle>)this.candles).GetEnumerator();

        public override bool Equals(object? obj) => obj is DescendingDays other && this.Equals(other);

        public override int GetHashCode() => this.candles.GetHashCode();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.candles).GetEnumerator();

        public sealed class Builder
        {
            private readonly ImmutableArray<Candle>.Builder inner = ImmutableArray.CreateBuilder<Candle>();

            public void Add(Candle candle)
            {
                Debug.Assert(this.inner.Count == 0 || this.inner.Last().Time > candle.Time, "this.inner.Count == 0 || this.inner.Last().Time> candle.Time");
                this.inner.Add(candle);
            }

            public DescendingDays Create() => new(this.inner.ToImmutable());
        }
    }
}
