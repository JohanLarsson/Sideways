namespace Sideways.Scan
{
    using System;

    public sealed class YieldCriteria : Criteria
    {
        private int days;
        private Percent? min;
        private Percent? max;

        public override string Info => (this.min, this.max) switch
        {
            // ReSharper disable LocalVariableHidesMember
            ({ } min, { } max) => $"[{min}..{max}] in {this.days} d",
            (null, { } max) => $"[..{max}] in {this.days} d",
            ({ } min, null) => $"[{min}..] in {this.days} d",
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

        public override bool IsSatisfied(SortedCandles candles, int index)
        {
            if (!this.IsActive)
            {
                return true;
            }

            // ReSharper disable LocalVariableHidesMember
            if (this.days is > 0 and var days)
            {
                return Percent.From(candles[index - days].Open, candles[index].Close).IsBetween(this.min ?? Percent.MinValue, this.max ?? Percent.MaxValue);
            }
            //// ReSharper restore LocalVariableHidesMember

            throw new InvalidOperationException($"{nameof(YieldCriteria)} expected days >= 1.");
        }
    }
}
