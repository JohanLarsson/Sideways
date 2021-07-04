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
        private readonly PriceCriteria priceCriteria = new();
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
            this.CurrentCriteria = new ReadOnlyObservableCollection<Criteria>(
                new ObservableCollection<Criteria>
                {
                    this.timeCriteria,
                    this.yieldCriteria,
                    this.priceCriteria,
                    this.adrCriteria,
                    this.averageVolumeCriteria,
                    this.averageDollarVolumeCriteria,
                    this.hasMinutes,
                });

            this.ScanCommand = new RelayCommand(_ => RunScan(), _ => this.CurrentCriteria.Any(x => x.IsActive));

            async void RunScan()
            {
                try
                {
                    this.IsRunning = true;
                    this.results.Clear();
                    var timeRange = this.timeCriteria is { IsActive: true, Start: var start, End: var end }
                        ? new TimeRange(
                            start?.AddDays(-2 * this.CurrentCriteria.Where(x => x.IsActive).Max(x => x.ExtraDays)) ?? DateTimeOffset.MinValue,
                            end ?? DateTimeOffset.MaxValue)
                        : (TimeRange?)null;
                    var symbols = await Task.Run(() => Database.ReadSymbols()).ConfigureAwait(true);
                    foreach (var symbol in symbols)
                    {
                        var bookmarks = await Task.Run(() => this.Scan(symbol, timeRange)).ConfigureAwait(true);
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

        public ReadOnlyObservableCollection<Criteria> CurrentCriteria { get; }

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

        public IEnumerable<Bookmark> Scan(string symbol, TimeRange? timeRange)
        {
            var days = timeRange is { Min: var start, Max: var end } ? Database.ReadDays(symbol, start, end) : Database.ReadDays(symbol);
            var firstMinute = this.hasMinutes.IsActive ? Database.FirstMinute(symbol) : null;
            for (var i = 0; i < days.Count; i++)
            {
                if (this.timeCriteria.IsSatisfied(days, i) &&
                    this.hasMinutes.IsSatisfied(days, i - (this.yieldCriteria.IsActive ? this.yieldCriteria.Days : 0), firstMinute) &&
                    this.yieldCriteria.IsSatisfied(days, i) &&
                    this.adrCriteria.IsSatisfied(days, i) &&
                    this.averageVolumeCriteria.IsSatisfied(days, i) &&
                    this.averageDollarVolumeCriteria.IsSatisfied(days, i))
                {
                    yield return new Bookmark(symbol, TradingDay.EndOfDay(days[i].Time), ImmutableSortedSet<string>.Empty, null);
                }
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
