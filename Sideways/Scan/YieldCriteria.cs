namespace Sideways.Scan
{
    using System;

    public sealed class YieldCriteria : Criteria
    {
        private int days = 5;
        private Percent? min = new Percent(25);
        private Percent? max;

        public override string Info => (this.min, this.max) switch
        {
            // ReSharper disable LocalVariableHidesMember
            (min: { } min, max: { } max) => $"{min:#.#} ≤ y ≤ {max:#.#}",
            (min: null, max: { } max) => $"y ≤ {max:#.#}",
            (min: { } min, max: null) => $"{min:#.#} ≤ y",
            (null, null) => $"Yield *",
            //// ReSharper restore LocalVariableHidesMember
        };

        public int Days
        {
            get => this.days;
            set
            {
                if (value == this.days)
                {
                    return;
                }

                this.days = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ExtraDays));
                this.OnPropertyChanged(nameof(this.Info));
            }
        }

        public Percent? Min
        {
            get => this.min;
            set
            {
                if (value == this.min)
                {
                    return;
                }

                this.min = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Info));
            }
        }

        public Percent? Max
        {
            get => this.max;
            set
            {
                if (value == this.max)
                {
                    return;
                }

                this.max = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Info));
            }
        }

        public override int ExtraDays => this.Days;

        public bool IsSatisfied(SortedCandles candles, int index)
        {
            if (!this.IsActive)
            {
                return true;
            }

            if (!candles.CanSlice(index, -this.days))
            {
                return false;
            }

            // ReSharper disable LocalVariableHidesMember
            if (this.days is > 0 and var days)
            {
                var merged = Candle.Merge(candles.Slice(index, -days));
                if (this.min is { Scalar: > 0 } &&
                    candles[index].High < merged.High)
                {
                    return false;
                }

                return Percent.Change(merged.Open, merged.High).IsBetween(this.min ?? Percent.MinValue, this.max ?? Percent.MaxValue);
            }
            //// ReSharper restore LocalVariableHidesMember

            throw new InvalidOperationException($"{nameof(YieldCriteria)} expected days >= 1.");
        }
    }
}
