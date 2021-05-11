namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;

    public readonly struct DescendingSplits : IEquatable<DescendingSplits>, IReadOnlyList<Split>
    {
        private readonly ImmutableArray<Split> splits;

        private DescendingSplits(ImmutableArray<Split> splits)
        {
            this.splits = splits;
        }

        public int Count => this.splits.Length;

        public Split this[int index] => this.splits[index];

        public static bool operator ==(DescendingSplits left, DescendingSplits right) => left.Equals(right);

        public static bool operator !=(DescendingSplits left, DescendingSplits right) => !left.Equals(right);

        public static Builder CreateBuilder() => new();

        public DescendingDays Adjust(DescendingDays days)
        {
            if (this.splits.IsEmpty)
            {
                return days;
            }

            return new DescendingDays(this.AdjustCore(days));
        }

        public DescendingMinutes Adjust(DescendingMinutes minutes)
        {
            if (this.splits.IsEmpty)
            {
                return minutes;
            }

            return new DescendingMinutes(this.AdjustCore(minutes));
        }

        public bool Equals(DescendingSplits other) => this.splits.Equals(other.splits);

        public IEnumerator<Split> GetEnumerator() => ((IEnumerable<Split>)this.splits).GetEnumerator();

        public override bool Equals(object? obj) => obj is DescendingSplits other && this.Equals(other);

        public override int GetHashCode() => this.splits.GetHashCode();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.splits).GetEnumerator();

        private ImmutableArray<Candle> AdjustCore(IReadOnlyList<Candle> raw)
        {
            var splitIndex = 0;
            var c = 1.0;
            var builder = ImmutableArray.CreateBuilder<Candle>(raw.Count);
            foreach (var candle in raw)
            {
                if (splitIndex < this.splits.Length &&
                    candle.Time < this.splits[splitIndex].Date)
                {
                    c /= this.splits[splitIndex].Coefficient;
                    splitIndex++;
                }

                builder.Add(candle.Adjust(c));
            }

            return builder.MoveToImmutable();
        }

        public sealed class Builder
        {
            private readonly ImmutableArray<Split>.Builder inner = ImmutableArray.CreateBuilder<Split>();

            public void Add(Split split)
            {
                Debug.Assert(this.inner.Count == 0 || this.inner.Last().Date > split.Date, "this.inner.Count == 0 || this.inner.Last().Date > split.Date");
                this.inner.Add(split);
            }

            public DescendingSplits Create() => new(this.inner.ToImmutable());
        }
    }
}
