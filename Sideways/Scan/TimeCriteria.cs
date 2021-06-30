namespace Sideways.Scan
{
    using System;

    public sealed class TimeCriteria : Criteria
    {
        private DateTimeOffset? start;
        private DateTimeOffset? end;

        public override string Info => (this.Start, this.End) switch
        {
            // ReSharper disable LocalVariableHidesMember
            (Start: { } start, End: { } end) => $"[{start:yyyy-MM-dd}..{end:yyyy-MM-dd}]",
            (Start: null, End: { } end) => $"[..{end:yyyy-MM-dd}]",
            (Start: { } start, End: null) => $"[{start:yyyy-MM-dd}..]",
            (null, null) => "??",
            //// ReSharper restore LocalVariableHidesMember
        };

        public DateTimeOffset? Start
        {
            get => this.start;
            set
            {
                if (value == this.start)
                {
                    return;
                }

                this.start = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Info));
            }
        }

        public DateTimeOffset? End
        {
            get => this.end;
            set
            {
                if (value == this.end)
                {
                    return;
                }

                this.end = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Info));
            }
        }

        public override bool IsSatisfied(SortedCandles candles, int index)
        {
            if (!this.IsActive)
            {
                throw new InvalidOperationException($"{nameof(YieldCriteria)} is not active.");
            }

            return candles[index].Time.IsBetween(this.start ?? DateTimeOffset.MinValue, this.end ?? DateTimeOffset.MaxValue);
        }
    }
}
