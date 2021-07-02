namespace Sideways.Scan
{
    using System;

    public sealed class TimeCriteria : Criteria
    {
        private DateTimeOffset? start = DateTimeOffset.Now.Date.AddMonths(-1);
        private DateTimeOffset? end;

        public override string Info => this switch
            {
                { YearToDate: true } => "YTD",
                { LastMonth: true } => "Last month",
                { LastWeek: true } => "Last week",
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
            }
        }

        public bool IsSatisfied(SortedCandles candles, int index)
        {
            return !this.IsActive ||
                   candles[index].Time.IsBetween(this.start ?? DateTimeOffset.MinValue, this.end ?? DateTimeOffset.MaxValue);
        }

        private static DateTimeOffset FirstDayOfYear(int year) => new(year, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
    }
}
