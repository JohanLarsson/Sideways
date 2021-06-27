namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public sealed class Scan : INotifyPropertyChanged
    {
        private double? minYield;
        private int? days;
        private double? minAdr;
        private double? minAverageVolume;
        private double? minAverageDollarVolume;
        private bool hasMinutes;
        private DateTimeOffset? startDate;
        private DateTimeOffset? endDate;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double? MinYield
        {
            get => this.minYield;
            set
            {
                if (value == this.minYield)
                {
                    return;
                }

                this.minYield = value;
                this.OnPropertyChanged();
            }
        }

        public int? Days
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
            }
        }

        public double? MinAdr
        {
            get => this.minAdr;
            set
            {
                if (value == this.minAdr)
                {
                    return;
                }

                this.minAdr = value;
                this.OnPropertyChanged();
            }
        }

        public double? MinAverageVolume
        {
            get => this.minAverageVolume;
            set
            {
                if (value == this.minAverageVolume)
                {
                    return;
                }

                this.minAverageVolume = value;
                this.OnPropertyChanged();
            }
        }

        public double? MinAverageDollarVolume
        {
            get => this.minAverageDollarVolume;
            set
            {
                if (value == this.minAverageDollarVolume)
                {
                    return;
                }

                this.minAverageDollarVolume = value;
                this.OnPropertyChanged();
            }
        }

        public bool HasMinutes
        {
            get => this.hasMinutes;
            set
            {
                if (value == this.hasMinutes)
                {
                    return;
                }

                this.hasMinutes = value;
                this.OnPropertyChanged();
            }
        }

        public DateTimeOffset? StartDate
        {
            get => this.startDate;
            set
            {
                if (value == this.startDate)
                {
                    return;
                }

                this.startDate = value;
                this.OnPropertyChanged();
            }
        }

        public DateTimeOffset? EndDate
        {
            get => this.endDate;
            set
            {
                if (value == this.endDate)
                {
                    return;
                }

                this.endDate = value;
                this.OnPropertyChanged();
            }
        }

        public IEnumerable<Bookmark> Run()
        {
            if (this.minYield is { } &&
                this.days is null)
            {
                throw new InvalidOperationException("days cannot be null when yield is specified.");
            }

            if (this.days is { } days)
            {
                if (this.minYield is null)
                {
                    throw new InvalidOperationException("yield cannot be null when days is specified.");
                }

                var min = 1 + (this.minYield / 100.0);
                foreach (var symbol in Database.ReadSymbols())
                {
                    if (Candles() is { } candles)
                    {
                        for (var i = Math.Max(20, days); i < candles.Count; i++)
                        {
                            if (candles[i].Close / candles[i - days].Close > min &&
                                SatisfiesMinAdr() &&
                                SatisfiesMinAverageVolume() &&
                                SatisfiesMinAverageDollarVolume())
                            {
                                yield return new Bookmark(symbol, TradingDay.StartOfDay(candles[i - 3].Time), ImmutableSortedSet<string>.Empty, null);
                            }

                            bool SatisfiesMinAdr()
                            {
                                return this.minAdr is null || Take20().Adr() > this.minAdr;
                            }

                            bool SatisfiesMinAverageVolume()
                            {
                                return this.minAverageVolume is null || Take20().Average(x => x.Volume) > this.minAverageVolume;
                            }

                            bool SatisfiesMinAverageDollarVolume()
                            {
                                return this.minAverageDollarVolume is null || Take20().Average(x => x.Volume * x.Close) > this.minAverageDollarVolume;
                            }

                            IEnumerable<Candle> Take20()
                            {
                                for (var j = 0; j < 20; j++)
                                {
                                    yield return candles[i - j];
                                }
                            }
                        }
                    }

                    SortedCandles? Candles()
                    {
                        if (MinTime() is { } minTime)
                        {
                            return Database.ReadDays(symbol, minTime, this.endDate ?? DateTimeOffset.Now);
                        }

                        if (this.endDate is { } maxTime)
                        {
                            return Database.ReadDays(symbol, DateTimeOffset.MinValue, maxTime);
                        }

                        return Database.ReadDays(symbol);

                        DateTimeOffset? MinTime()
                        {
                            if (this.hasMinutes)
                            {
                                if (Database.FirstMinute(symbol) is { } firstMinute)
                                {
                                    if (this.startDate is { } minTime)
                                    {
                                        return DateTimeOffsetExtensions.Min(firstMinute, minTime);
                                    }

                                    return firstMinute;
                                }
                            }

                            return this.startDate;
                        }
                    }
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
