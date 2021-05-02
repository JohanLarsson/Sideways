namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Sideways.AlphaVantage;

    public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly string ApiKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways/AlphaVantage.key");
        private readonly DataSource dataSource = new(new HttpClientHandler(), ApiKey());
        private string? symbol;
        private SortedCandles? weeks;
        private SortedCandles? days;
        private SortedCandles? hours;
        private SortedCandles? minutes;
        private DateTimeOffset endTime = DateTimeOffset.Now;
        private bool disposed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Symbol
        {
            get => this.symbol;
            set
            {
                if (value == this.symbol)
                {
                    return;
                }

                this.symbol = value;
                this.OnPropertyChanged();
            }
        }

        public SortedCandles? Weeks
        {
            get => this.weeks;
            set
            {
                if (ReferenceEquals(value, this.weeks))
                {
                    return;
                }

                this.weeks = value;
                this.OnPropertyChanged();
            }
        }

        public SortedCandles? Days
        {
            get => this.days;
            set
            {
                if (ReferenceEquals(value, this.days))
                {
                    return;
                }

                this.days = value;
                this.OnPropertyChanged();
            }
        }

        public SortedCandles? Hours
        {
            get => this.hours;
            set
            {
                if (ReferenceEquals(value, this.hours))
                {
                    return;
                }

                this.hours = value;
                this.OnPropertyChanged();
            }
        }

        public SortedCandles? Minutes
        {
            get => this.minutes;
            set
            {
                if (ReferenceEquals(value, this.minutes))
                {
                    return;
                }

                this.minutes = value;
                this.OnPropertyChanged();
            }
        }

        public DateTimeOffset EndTime
        {
            get => this.endTime;
            set
            {
                if (value == this.endTime)
                {
                    return;
                }

                this.endTime = value;
                this.OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.dataSource.Dispose();
        }

        public async Task LoadAsync(string symbol)
        {
            this.Symbol = symbol;
            var adjustedDays = await this.dataSource.DaysAsync(symbol).ConfigureAwait(false);
            var days = new SortedCandles(Days());
            this.Days = days;
            this.Weeks = new SortedCandles(Weeks());
            var minutes = new SortedCandles(await this.dataSource.MinutesAsync(symbol).ConfigureAwait(false));
            this.Minutes = minutes;
            this.Hours = new SortedCandles(Hours());

            IEnumerable<Candle> Days()
            {
                var splitCoefficient = 1f;
                foreach (var day in adjustedDays)
                {
                    if (day.SplitCoefficient != 0)
                    {
                        splitCoefficient *= day.SplitCoefficient;
                    }

                    yield return day.AsCandle(splitCoefficient);
                }
            }

            IEnumerable<Candle> Weeks()
            {
                var week = new List<Candle>();
                foreach (var day in days)
                {
                    if (week.Count == 0 ||
                        Week(week[0]) == Week(day))
                    {
                        week.Add(day);
                    }
                    else
                    {
                        if (week.Count > 0)
                        {
                            yield return CreateWeek();
                        }

                        week.Clear();
                        week.Add(day);
                    }

                    static int Week(Candle c) => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(c.Time.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                }

                if (week.Count > 0)
                {
                    yield return CreateWeek();
                }

                Candle CreateWeek() => new(
                    week.Last().Time,
                    week[0].Open,
                    week.Max(x => x.High),
                    week.Min(x => x.Low),
                    week[^1].Close,
                    week.Sum(x => x.Volume));
            }

            IEnumerable<Candle> Hours()
            {
                var hour = new List<Candle>();
                foreach (var minute in minutes)
                {
                    if (hour.Count == 0 ||
                        Hour(hour[0]) == Hour(minute))
                    {
                        hour.Add(minute);
                    }
                    else
                    {
                        if (hour.Count > 0)
                        {
                            yield return CreateHour();
                        }

                        hour.Clear();
                        hour.Add(minute);
                    }

                    static int Hour(Candle c) => c.Time.DayOfYear + c.Time.Hour;
                }

                if (hour.Count > 0)
                {
                    yield return CreateHour();
                }

                Candle CreateHour() => new(
                    hour.Last().Time,
                    hour[0].Open,
                    hour.Max(x => x.High),
                    hour.Min(x => x.Low),
                    hour[^1].Close,
                    hour.Sum(x => x.Volume));
            }
        }

        private static string ApiKey()
        {
            if (File.Exists(ApiKeyFile))
            {
                return File.ReadAllText(ApiKeyFile).Trim();
            }

            throw new InvalidOperationException($"Missing file {ApiKeyFile}");
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(MainViewModel));
            }
        }
    }
}
