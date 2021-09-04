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

    using Sideways.AlphaVantage;
    using Sideways.Scan;

    public sealed class ScanViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<Bookmark> results = new();
        private readonly TimeCriteria timeCriteria = new();
        private readonly PriceCriteria priceCriteria = new();
        private readonly YieldCriteria yieldCriteria = new();
        private readonly GapCriteria gapCriteria = new();
        private readonly AdrCriteria adrCriteria = new();
        private readonly AverageVolumeCriteria averageVolumeCriteria = new();
        private readonly AverageDollarVolumeCriteria averageDollarVolumeCriteria = new();
        private readonly HasMinutesCriteria hasMinutes = new();

        private Bookmark? selectedResult;
        private bool isRunning;
        private int offset;

        public ScanViewModel(Downloader downloader)
        {
            this.Downloader = downloader;
            this.Results = new ReadOnlyObservableCollection<Bookmark>(this.results);
            this.CurrentCriteria = new ReadOnlyObservableCollection<Criteria>(
                new ObservableCollection<Criteria>
                {
                    this.timeCriteria,
                    this.yieldCriteria,
                    this.priceCriteria,
                    this.gapCriteria,
                    this.adrCriteria,
                    this.averageVolumeCriteria,
                    this.averageDollarVolumeCriteria,
                    this.hasMinutes,
                });

            this.ScanCommand = new RelayCommand(_ => RunScan(), _ => this.CurrentCriteria.Any(x => x.IsActive));
            this.DownloadAllCommand = new RelayCommand(
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _ => downloader.DownloadAllAsync(d => this.results.Any(x => x.Symbol == d.Symbol)),
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _ => this.results.Count > 0 &&
                     downloader is { SymbolDownloads: { IsEmpty: false }, SymbolDownloadState: { Status: DownloadStatus.Waiting or DownloadStatus.Completed or DownloadStatus.Error } });

            async void RunScan()
            {
                try
                {
                    if (this.isRunning)
                    {
                        this.isRunning = false;
                        await Task.Delay(TimeSpan.FromMilliseconds(100)).ConfigureAwait(true);
                    }

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
                        if (!this.isRunning)
                        {
                            return;
                        }

                        var bookmarks = await Task.Run(() => this.Scan(symbol, timeRange)).ConfigureAwait(true);
                        foreach (var bookmark in bookmarks)
                        {
                            if (!this.isRunning)
                            {
                                return;
                            }

                            this.results.Add(bookmark);
                        }
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    _ = MessageBox.Show(e.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    this.IsRunning = false;
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand ScanCommand { get; }

        public ICommand DownloadAllCommand { get; }

        public Downloader Downloader { get; }

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
            var days = timeRange is { Min: var start, Max: var end }
                ? Database.ReadDays(symbol, start, TradingDay.EndOfDay(end))
                : Database.ReadDays(symbol);
            days = Database.ReadSplits(symbol).Adjust(days);
            var firstMinute = this.hasMinutes.IsActive ? Database.FirstMinute(symbol) : null;
            for (var i = 0; i < days.Count; i++)
            {
                if (!this.isRunning)
                {
                    yield break;
                }

                if (IsSatisfied(i))
                {
                    if (this.yieldCriteria.IsActive &&
                        !this.gapCriteria.IsActive &&
                        i < days.Count - 1 &&
                        IsSatisfied(i + 1))
                    {
                        // Sample next match to avoid consecutive.
                        continue;
                    }
                    else
                    {
                        yield return new Bookmark(symbol, TradingDay.EndOfDay(days[i].Time), ImmutableSortedSet<string>.Empty, null);
                    }
                }
            }

            bool IsSatisfied(int index)
            {
                return this.timeCriteria.IsSatisfied(days, index) &&
                       this.hasMinutes.IsSatisfied(days, index, firstMinute) &&
                       this.yieldCriteria.IsSatisfied(days, index) &&
                       this.priceCriteria.IsSatisfied(days, index) &&
                       this.gapCriteria.IsSatisfied(days, index) &&
                       this.adrCriteria.IsSatisfied(days, index) &&
                       this.averageVolumeCriteria.IsSatisfied(days, index) &&
                       this.averageDollarVolumeCriteria.IsSatisfied(days, index);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
