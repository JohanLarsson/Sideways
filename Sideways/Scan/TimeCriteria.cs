namespace Sideways.Scan
{
    using System;
    using System.Globalization;
    using System.Windows.Input;

    public sealed class TimeCriteria : Criteria
    {
        private DateTimeOffset? start = DateTimeOffset.Now.Date.AddMonths(-1);
        private DateTimeOffset? end;

        public TimeCriteria()
        {
            this.NextYearCommand = new RelayCommand(
                _ => this.SetYear(this.EffectiveYear() + 1),
                _ => this.EffectiveYear() < DateTime.Today.Year);
            this.PreviousYearCommand = new RelayCommand(_ => this.SetYear(this.EffectiveYear() - 1));
            this.NextMonthCommand = new RelayCommand(
                _ => this.SetMonth(this.EffectiveYear(), this.EffectiveMonth() + 1),
                _ => this.EffectiveYear() < DateTime.Today.Year || this.EffectiveMonth() < DateTime.Today.Month);
            this.PreviousMonthCommand = new RelayCommand(_ => this.SetMonth(this.EffectiveYear(), this.EffectiveMonth() - 1));
        }

        public ICommand NextYearCommand { get; }

        public ICommand PreviousYearCommand { get; }

        public ICommand NextMonthCommand { get; }

        public ICommand PreviousMonthCommand { get; }

        public string YearText => this.EffectiveYear().ToString(CultureInfo.InvariantCulture);

        public string MonthText =>
#pragma warning disable CA1305 // Specify IFormatProvider
            new DateTimeOffset(this.EffectiveYear(), this.EffectiveMonth(), 1, 0, 0, 0, 0, TimeSpan.Zero).ToString("MMM yyyy");
#pragma warning restore CA1305 // Specify IFormatProvider

        public override string Info => this switch
        {
            { Year: true } => this.YearText,
            { Month: true } => this.MonthText,
            { LastWeek: true } => "Week",
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

        public bool Year
        {
            get => (this.start, this.end) switch
            {
                // ReSharper disable LocalVariableHidesMember
                ({ } start, { } end)
                    => start.Year == end.Year &&
                       start == FirstDayOfYear(start.Year) &&
                       end == LastDayOfYear(end.Year),
                ({ } start, null)
                    => start == FirstDayOfYear(start.Year),
                //// ReSharper restore LocalVariableHidesMember
                _ => false,
            };

            set
            {
                if (value is true)
                {
                    this.SetYear(this.EffectiveYear());
                }
            }
        }

        public bool Month
        {
            get =>
                (this.start, this.end) switch
                {
                    // ReSharper disable LocalVariableHidesMember
                    ({ } start, { } end)
                        => start.Year == end.Year &&
                           start.Month == end.Month &&
                           start.Day == 1 &&
                           end.AddDays(1).Month != end.Month,
                    ({ } start, null)
                        => start.Day == 1 &&
                           start.Month == DateTime.Today.Month,
                    //// ReSharper restore LocalVariableHidesMember
                    _ => false,
                };

            set
            {
                if (value is true)
                {
                    this.SetMonth(this.EffectiveYear(), this.EffectiveMonth());
                }
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
                this.OnPropertyChanged(nameof(this.YearText));
                this.OnPropertyChanged(nameof(this.MonthText));
                this.OnPropertyChanged(nameof(this.Info));
                this.OnPropertyChanged(nameof(this.LastWeek));
                this.OnPropertyChanged(nameof(this.Year));
                this.OnPropertyChanged(nameof(this.Month));
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
                this.OnPropertyChanged(nameof(this.YearText));
                this.OnPropertyChanged(nameof(this.MonthText));
                this.OnPropertyChanged(nameof(this.Info));
                this.OnPropertyChanged(nameof(this.LastWeek));
                this.OnPropertyChanged(nameof(this.Year));
                this.OnPropertyChanged(nameof(this.Month));
            }
        }

        public bool IsSatisfied(SortedCandles candles, int index)
        {
            return !this.IsActive ||
                   candles[index].Time.IsBetween(this.start ?? DateTimeOffset.MinValue, this.end ?? DateTimeOffset.MaxValue);
        }

        private static DateTimeOffset FirstDayOfYear(int year) => new(year, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        private static DateTimeOffset LastDayOfYear(int year) => new(year, 12, 31, 0, 0, 0, 0, TimeSpan.Zero);

        private static DateTimeOffset LastDayOfMonth(int year, int month)
        {
            var day = new DateTimeOffset(year, month, 28, 0, 0, 0, 0, TimeSpan.Zero);
            while (day.AddDays(1).Month == day.Month)
            {
                day = day.AddDays(1);
            }

            return day;
        }

        private int EffectiveYear() => this.start?.Year ?? this.end?.Year ?? DateTime.Today.Year;

        private int EffectiveMonth() => this.start?.Month ?? this.end?.Month ?? DateTime.Today.Month;

        private void SetYear(int year)
        {
            this.Start = FirstDayOfYear(year);
            this.End = LastDayOfYear(year);
        }

        private void SetMonth(int year, int month)
        {
            if (month == 0)
            {
                year--;
                month = 12;
            }

            if (month == 13)
            {
                year++;
                month = 1;
            }

            this.Start = new(year, month, 1, 0, 0, 0, 0, TimeSpan.Zero);
            this.End = LastDayOfMonth(year, month);
        }
    }
}
