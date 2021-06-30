namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    public sealed class ScanViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<Bookmark> results = new();

        private Bookmark? selectedResult;
        private double? minYield;
        private int? days;
        private double? minAdr;
        private double? minAverageVolume;
        private double? minAverageDollarVolume;
        private bool hasMinutes;
        private DateTimeOffset? startDate;
        private DateTimeOffset? endDate;
        private int offset;

        public ScanViewModel()
        {
            this.Results = new ReadOnlyObservableCollection<Bookmark>(this.results);
            this.ScanCommand = new RelayCommand(_ => RunScan());

            async void RunScan()
            {
                try
                {
                    this.results.Clear();
                    var symbols = await Task.Run(() => Database.ReadSymbols()).ConfigureAwait(true);
                    foreach (var symbol in symbols)
                    {
                        var bookmarks = await Task.Run(() => this.Scan(symbol)).ConfigureAwait(true);
                        foreach (var bookmark in bookmarks)
                        {
                            this.results.Add(bookmark);
                        }
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    _ = MessageBox.Show(e.Message, "Scan", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand ScanCommand { get; }

        public ReadOnlyObservableCollection<Bookmark> Results { get; }

        public Bookmark? SelectedResult
        {
            get => this.selectedResult;
            set
            {
                if (ReferenceEquals(value, this.selectedResult))
                {
                    return;
                }

                this.selectedResult = value;
                this.OnPropertyChanged();
            }
        }

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

        public int Offset
        {
            get => this.offset;
            set
            {
                if (value == this.offset)
                {
                    return;
                }

                this.offset = value;
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
                                MinAdrOk() &&
                                MinAverageVolumeOk() &&
                                MinAverageDollarVolumeOk())
                            {
                                yield return new Bookmark(symbol, TradingDay.StartOfDay(candles[i - days].Time), ImmutableSortedSet<string>.Empty, null);
                            }

                            bool MinAdrOk()
                            {
                                return this.minAdr is null || Take20().Adr() > this.minAdr;
                            }

                            bool MinAverageVolumeOk()
                            {
                                return this.minAverageVolume is null || Take20().Average(x => x.Volume) > this.minAverageVolume;
                            }

                            bool MinAverageDollarVolumeOk()
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

        public IEnumerable<Bookmark> Scan(string symbol)
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
                if (Candles() is { } candles)
                {
                    for (var i = Math.Max(20, days); i < candles.Count; i++)
                    {
                        if (candles[i].Close / candles[i - days].Close > min &&
                            MinAdrOk() &&
                            MinAverageVolumeOk() &&
                            MinAverageDollarVolumeOk())
                        {
                            yield return new Bookmark(symbol, TradingDay.StartOfDay(candles[i - days].Time), ImmutableSortedSet<string>.Empty, null);
                        }

                        bool MinAdrOk()
                        {
                            return this.minAdr is null || Take20().Adr() > this.minAdr;
                        }

                        bool MinAverageVolumeOk()
                        {
                            return this.minAverageVolume is null || Take20().Average(x => x.Volume) > this.minAverageVolume;
                        }

                        bool MinAverageDollarVolumeOk()
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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
