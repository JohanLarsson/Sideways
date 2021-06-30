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

    using Sideways.Scan;

    public sealed class ScanViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<Bookmark> results = new();
        private readonly TimeCriteria timeCriteria = new();
        private readonly YieldCriteria yieldCriteria = new();
        private readonly AdrCriteria adrCriteria = new();
        private readonly AverageVolumeCriteria averageVolumeCriteria = new();
        private readonly AverageDollarVolumeCriteria averageDollarVolumeCriteria = new();
        private readonly HasMinutesCriteria hasMinutes = new();

        private Bookmark? selectedResult;
        private bool isRunning;
        private int offset;

        public ScanViewModel()
        {
            this.Results = new ReadOnlyObservableCollection<Bookmark>(this.results);
            this.CurrentCriteria = new ObservableCollection<Criteria>
            {
                this.timeCriteria,
                this.yieldCriteria,
                this.adrCriteria,
                this.averageVolumeCriteria,
                this.averageDollarVolumeCriteria,
                this.hasMinutes,
            };

            this.ScanCommand = new RelayCommand(_ => RunScan(), _ => this.CurrentCriteria.Any(x => x.IsActive));

            async void RunScan()
            {
                try
                {
                    this.IsRunning = true;
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
                finally
                {
                    this.IsRunning = false;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand ScanCommand { get; }

        public ObservableCollection<Criteria> CurrentCriteria { get; }

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

        public bool IsRunning
        {
            get => this.isRunning;
            private set
            {
                if (value == this.isRunning)
                {
                    return;
                }

                this.isRunning = value;
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

        public IEnumerable<Bookmark> Scan(string symbol)
        {
            var start = this.CurrentCriteria.Where(x => x.IsActive).Max(x => x.ExtraDays);
            if (Candles() is { } candles)
            {
                for (var i = start; i < candles.Count; i++)
                {
                    if (this.yieldCriteria.IsSatisfied(candles, i) &&
                        this.adrCriteria.IsSatisfied(candles, i) &&
                        this.averageVolumeCriteria.IsSatisfied(candles, i) &&
                        this.averageDollarVolumeCriteria.IsSatisfied(candles, i))
                    {
                        yield return new Bookmark(symbol, TradingDay.StartOfDay(candles[i - this.yieldCriteria.Days].Time), ImmutableSortedSet<string>.Empty, null);
                    }
                }
            }

            SortedCandles? Candles()
            {
                // ReSharper disable VariableHidesOuterVariable
                if (Start() is { } start)
                {
                    return Database.ReadDays(symbol, start, this.timeCriteria.End ?? DateTimeOffset.Now);
                }

                if (this.timeCriteria.End is { } end)
                {
                    return Database.ReadDays(symbol, DateTimeOffset.MinValue, end);
                }

                return Database.ReadDays(symbol);

                DateTimeOffset? Start()
                {
                    return (this.hasMinutes.IsActive, this.timeCriteria) switch
                    {
                        (true, { IsActive: true, Start: { } start })
                            when Database.FirstMinute(symbol) is { } first
                            => DateTimeOffsetExtensions.Min(first, start),
                        (true, { IsActive: true, Start: { } })
                            => null,
                        (true, { IsActive: false }) => Database.FirstMinute(symbol),
                        (true, { Start: null }) => Database.FirstMinute(symbol),
                        (_, { IsActive: true, Start: { } start }) => start,
                        _ => null,
                    };
                }
                //// ReSharper restore VariableHidesOuterVariable
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
