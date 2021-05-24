namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;

    public readonly struct SortedSplits : IEquatable<SortedSplits>, IReadOnlyList<Split>
    {
        private readonly ImmutableArray<Split> splits;

        private SortedSplits(ImmutableArray<Split> splits)
        {
            this.splits = splits;
        }

        public int Count => this.splits.Length;

        public Split this[int index] => this.splits[index];

        public static bool operator ==(SortedSplits left, SortedSplits right) => left.Equals(right);

        public static bool operator !=(SortedSplits left, SortedSplits right) => !left.Equals(right);

        public static Builder CreateBuilder() => new();

        public SortedCandles Adjust(SortedCandles candles)
        {
            if (this.splits.IsEmpty)
            {
                return candles;
            }

            return candles.AdjustBy(this);
        }

        public bool Equals(SortedSplits other) => this.splits.Equals(other.splits);

        public IEnumerator<Split> GetEnumerator() => ((IEnumerable<Split>)this.splits).GetEnumerator();

        public override bool Equals(object? obj) => obj is SortedSplits other && this.Equals(other);

        public override int GetHashCode() => this.splits.GetHashCode();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.splits).GetEnumerator();

#pragma warning disable CA1034 // Nested types should not be visible
        public sealed class Builder
#pragma warning restore CA1034 // Nested types should not be visible
        {
            private readonly ImmutableArray<Split>.Builder inner = ImmutableArray.CreateBuilder<Split>();

            public void Add(Split split)
            {
                Debug.Assert(this.inner.Count == 0 || this.inner[^1].Date < split.Date, "Must be ascending.");
                this.inner.Add(split);
            }

            public SortedSplits Create() => new(this.inner.ToImmutable());
        }
    }
}
