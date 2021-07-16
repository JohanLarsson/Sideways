namespace Sideways.Scan
{
    using System;

    public sealed class YieldCriteria : Criteria
    {
        private int days = 5;
        private Percent? min = new Percent(25);
        private Percent? max;

        public static string FifteenPercentInThreeDaysText { get; } = InfoText(new Percent(15), null, 3);

        public static string TwentyFivePercentInFiveDaysText { get; } = InfoText(new Percent(25), null, 5);

        public static string FiftyPercentInFiftyDaysText { get; } = InfoText(new Percent(50), null, 50);

        public static string EightyPercentInEightyDaysText { get; } = InfoText(new Percent(80), null, 80);

        public override string Info => InfoText(this.min, this.max, this.days);

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
                this.OnPropertyChanged(nameof(this.FifteenPercentInThreeDays));
                this.OnPropertyChanged(nameof(this.TwentyFivePercentInFiveDays));
                this.OnPropertyChanged(nameof(this.FiftyPercentInFiftyDays));
                this.OnPropertyChanged(nameof(this.ExtraDays));
                this.OnPropertyChanged(nameof(this.Info));
                this.OnPropertyChanged(nameof(this.EightyPercentInEightyDays));
            }
        }

        public override int ExtraDays => this.Days;

        public bool FifteenPercentInThreeDays
        {
            get => this is { min: { Scalar: 15 }, max: null, days: 3 };
            set
            {
                if (!value ||
                    value == this.FifteenPercentInThreeDays)
                {
                    return;
                }

                this.Min = new Percent(15);
                this.Max = null;
                this.Days = 3;
            }
        }

        public bool TwentyFivePercentInFiveDays
        {
            get => this is { min: { Scalar: 25 }, max: null, days: 5 };
            set
            {
                if (!value ||
                    value == this.TwentyFivePercentInFiveDays)
                {
                    return;
                }

                this.Min = new Percent(25);
                this.Max = null;
                this.Days = 5;
            }
        }

        public bool FiftyPercentInFiftyDays
        {
            get => this is { min: { Scalar: 50 }, max: null, days: 50 };
            set
            {
                if (!value ||
                    value == this.FiftyPercentInFiftyDays)
                {
                    return;
                }

                this.Min = new Percent(50);
                this.Max = null;
                this.Days = 50;
            }
        }

        public bool EightyPercentInEightyDays
        {
            get => this is { min: { Scalar: 80 }, max: null, days: 80 };
            set
            {
                if (!value ||
                    value == this.EightyPercentInEightyDays)
                {
                    return;
                }

                this.Min = new Percent(80);
                this.Max = null;
                this.Days = 80;
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
                this.OnPropertyChanged(nameof(this.FifteenPercentInThreeDays));
                this.OnPropertyChanged(nameof(this.TwentyFivePercentInFiveDays));
                this.OnPropertyChanged(nameof(this.FiftyPercentInFiftyDays));
                this.OnPropertyChanged(nameof(this.EightyPercentInEightyDays));
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
                this.OnPropertyChanged(nameof(this.FifteenPercentInThreeDays));
                this.OnPropertyChanged(nameof(this.TwentyFivePercentInFiveDays));
                this.OnPropertyChanged(nameof(this.FiftyPercentInFiftyDays));
                this.OnPropertyChanged(nameof(this.EightyPercentInEightyDays));
            }
        }

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

        private static string InfoText(Percent? minimum, Percent? maximum, int days) => (minimum, maximum) switch
        {
            // ReSharper disable LocalVariableHidesMember
            (minimum: { } min, maximum: { } max) => $"{min:#.#} ≤ {days} d ≤ {max:#.#}",
            (minimum: null, maximum: { } max) => $"{days} d ≤ {max:#.#}",
            (minimum: { } min, maximum: null) => $"{min:#.#} ≤ {days} d",
            (null, null) => $"Yield *",
            //// ReSharper restore LocalVariableHidesMember
        };
    }
}
