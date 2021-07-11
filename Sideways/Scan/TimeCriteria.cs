namespace Sideways.Scan
{
    using System;
    using System.Globalization;

    public sealed class TimeCriteria : Criteria
    {
        private DateTimeOffset? start = DateTimeOffset.Now.Date.AddMonths(-1);
        private DateTimeOffset? end;

        public static string Year1Text => (DateTime.Today.Year - 1).ToString(CultureInfo.InvariantCulture);

        public static string Year2Text => (DateTime.Today.Year - 2).ToString(CultureInfo.InvariantCulture);

        public static string Year3Text => (DateTime.Today.Year - 3).ToString(CultureInfo.InvariantCulture);

        public static string Year4Text => (DateTime.Today.Year - 4).ToString(CultureInfo.InvariantCulture);

        public override string Info => this switch
        {
            { YearToDate: true } => "YTD",
            { LastMonth: true } => "Month",
            { LastWeek: true } => "Week",
            { Year1: true } => Year1Text,
            { Year2: true } => Year2Text,
            { Year3: true } => Year3Text,
            { Year4: true } => Year4Text,
            _ => (this.Start, this.End) switch
            {
                // ReSharper disable LocalVariableHidesMember
                (Start: { } start, End: { } end) => $"[{start:yyyy-MM-dd}..{end:yyyy-MM-dd}]",
                (Start: null, End: { } end) => $"[..{end:yyyy-MM-dd}]",
                (Start: { } start, End: null) => $"[{start:yyyy-MM-dd}..]",
                (null, null) => "Time *",
                //// ReSharper restore LocalVariableHidesMember
            },
        };

        // ReSharper disable once InconsistentNaming
        public bool YearToDate
        {
            get => this.start == FirstDayOfYear(DateTimeOffset.Now.Year) && this.end is null;
            set
            {
                if (!value ||
                    value == this.YearToDate)
                {
                    return;
                }

                this.Start = FirstDayOfYear(DateTimeOffset.Now.Year);
                this.End = null;
            }
        }

        public bool LastMonth
        {
            get => this.start == DateTimeOffset.Now.AddMonths(-1).Date && this.end is null;
            set
            {
                if (!value ||
                    value == this.LastMonth)
                {
                    return;
                }

                this.Start = DateTimeOffset.Now.AddMonths(-1).Date;
                this.End = null;
            }
        }

        public bool LastWeek
        {
            get => this.start == DateTimeOffset.Now.AddDays(-7).Date && this.end is null;
            set
            {
                if (!value ||
                    value == this.LastWeek)
                {
                    return;
                }

                this.Start = DateTimeOffset.Now.AddDays(-7).Date;
                this.End = null;
            }
        }

        public bool Year1
        {
            get => this.start == FirstDayOfYear(DateTime.Today.Year - 1) && this.end == LastDayOfYear(DateTime.Today.Year - 1);
            set
            {
                if (!value ||
                    value == this.Year1)
                {
                    return;
                }

                this.Start = FirstDayOfYear(DateTime.Today.Year - 1);
                this.End = LastDayOfYear(DateTime.Today.Year - 1);
            }
        }

        public bool Year2
        {
            get => this.start == FirstDayOfYear(DateTime.Today.Year - 2) && this.end == LastDayOfYear(DateTime.Today.Year - 2);
            set
            {
                if (!value ||
                    value == this.Year2)
                {
                    return;
                }

                this.Start = FirstDayOfYear(DateTime.Today.Year - 2);
                this.End = LastDayOfYear(DateTime.Today.Year - 2);
            }
        }

        public bool Year3
        {
            get => this.start == FirstDayOfYear(DateTime.Today.Year - 3) && this.end == LastDayOfYear(DateTime.Today.Year - 3);
            set
            {
                if (!value ||
                    value == this.Year3)
                {
                    return;
                }

                this.Start = FirstDayOfYear(DateTime.Today.Year - 3);
                this.End = LastDayOfYear(DateTime.Today.Year - 3);
            }
        }

        public bool Year4
        {
            get => this.start == FirstDayOfYear(DateTime.Today.Year - 4) && this.end == LastDayOfYear(DateTime.Today.Year - 4);
            set
            {
                if (!value ||
                    value == this.Year4)
                {
                    return;
                }

                this.Start = FirstDayOfYear(DateTime.Today.Year - 4);
                this.End = LastDayOfYear(DateTime.Today.Year - 4);
            }
        }

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
                this.OnPropertyChanged(nameof(this.YearToDate));
                this.OnPropertyChanged(nameof(this.LastMonth));
                this.OnPropertyChanged(nameof(this.LastWeek));
                this.OnPropertyChanged(nameof(this.Year1));
                this.OnPropertyChanged(nameof(this.Year2));
                this.OnPropertyChanged(nameof(this.Year3));
                this.OnPropertyChanged(nameof(this.Year4));
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
                this.OnPropertyChanged(nameof(this.YearToDate));
                this.OnPropertyChanged(nameof(this.LastMonth));
                this.OnPropertyChanged(nameof(this.LastWeek));
                this.OnPropertyChanged(nameof(this.Year1));
                this.OnPropertyChanged(nameof(this.Year2));
                this.OnPropertyChanged(nameof(this.Year3));
                this.OnPropertyChanged(nameof(this.Year4));
            }
        }

        public bool IsSatisfied(SortedCandles candles, int index)
        {
            return !this.IsActive ||
                   candles[index].Time.IsBetween(this.start ?? DateTimeOffset.MinValue, this.end ?? DateTimeOffset.MaxValue);
        }

        private static DateTimeOffset FirstDayOfYear(int year) => new(year, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        private static DateTimeOffset LastDayOfYear(int year) => new(year, 12, 31, 0, 0, 0, 0, TimeSpan.Zero);
    }
}
