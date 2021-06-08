namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Sideways;

    public sealed class Downloader : IDisposable, INotifyPropertyChanged
    {
        private readonly Settings settings;
        private AlphaVantageClient? client;

        private ImmutableList<Download> downloads = ImmutableList<Download>.Empty;
        private ImmutableList<SymbolDownloads> symbolDownloads = ImmutableList<SymbolDownloads>.Empty;
        private DownloadState symbolDownloadState = new();
        private bool disposed;

        public Downloader(Settings settings)
        {
            this.settings = settings;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.RefreshSymbolsCommand = new RelayCommand(_ => this.RefreshSymbolDownloadsAsync());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.DownloadAllSymbolsCommand = new RelayCommand(_ => DownloadAllTopUps(), _ => !this.symbolDownloads.IsEmpty);

            async void DownloadAllTopUps()
            {
                this.SymbolDownloadState.Start = DateTimeOffset.Now;
                foreach (var topUp in this.symbolDownloads)
                {
                    if (topUp.DownloadCommand.CanExecute(null))
                    {
                        await topUp.DownloadAsync().ConfigureAwait(false);
                    }
                }

                this.symbolDownloadState.Exception = this.symbolDownloads.FirstOrDefault(x => x.State.Exception is { })?.State.Exception;
                this.SymbolDownloadState.End = DateTimeOffset.Now;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<string>? NewSymbol;

        public event EventHandler<string>? NewDays;

        public event EventHandler<string>? NewMinutes;

        public ICommand RefreshSymbolsCommand { get; }

        public ICommand DownloadAllSymbolsCommand { get; }

        public AlphaVantageClient Client
        {
            get
            {
                if (this.client is { })
                {
                    return this.client;
                }

                if (this.settings is { AlphaVantage: { ClientSettings: { ApiKey: { } apiKey, MaxCallsPerMinute: var maxCallsPerMinute } } })
                {
                    return this.client ??= new(new HttpClientHandler(), apiKey, maxCallsPerMinute);
                }

                throw new InvalidOperationException("Missing AlphaVantage settings. Configure it first and try again.");
            }
        }

        public ImmutableList<Download> Downloads
        {
            get => this.downloads;
            private set
            {
                if (ReferenceEquals(value, this.downloads))
                {
                    return;
                }

                this.downloads = value;
                this.OnPropertyChanged();
            }
        }

        public ImmutableList<SymbolDownloads> SymbolDownloads
        {
            get => this.symbolDownloads;
            private set
            {
                if (ReferenceEquals(value, this.symbolDownloads))
                {
                    return;
                }

                this.symbolDownloads = value;
                this.OnPropertyChanged();
            }
        }

        public DownloadState SymbolDownloadState
        {
            get => this.symbolDownloadState;
            private set
            {
                if (value == this.symbolDownloadState)
                {
                    return;
                }

                this.symbolDownloadState = value;
                this.OnPropertyChanged();
            }
        }

        public async Task RefreshSymbolDownloadsAsync()
        {
            this.SymbolDownloads = ImmutableList<SymbolDownloads>.Empty;
            this.SymbolDownloadState = new DownloadState();
            var downloads = await Task.Run(Create).ConfigureAwait(false);
            this.SymbolDownloads = downloads
                 .OrderBy(x => x, Comparer<SymbolDownloads>.Create((x, y) => Compare(x, y)))
                 .ToImmutableList();

            IEnumerable<SymbolDownloads> Create()
            {
                var symbols = Database.ReadSymbols();
                var dayRanges = Database.DayRanges(symbols);
                var minuteRanges = Database.MinuteRanges(symbols);

                FixAlphaVantageSettings();

                foreach (var (symbol, dayRange) in dayRanges)
                {
                    if (AlphaVantage.SymbolDownloads.TryCreate(symbol, dayRange, minuteRanges.GetValueOrDefault(symbol), this, this.settings.AlphaVantage) is { } symbolDownloads)
                    {
                        yield return symbolDownloads;
                    }
                }

                void FixAlphaVantageSettings()
                {
                    foreach (var (symbol, range) in minuteRanges)
                    {
                        // Migrate settings in case symbol was marked as missing minutes due to empty old slice.
                        if (range != default &&
                            this.settings.AlphaVantage.SymbolsWithMissingMinutes.Contains(symbol))
                        {
                            this.settings.AlphaVantage.HasMinutes(symbol);
                            this.settings.Save();
                        }
                    }
                }
            }

            int Compare(SymbolDownloads? x, SymbolDownloads? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (y is null)
                {
                    return 1;
                }

                if (x is null)
                {
                    return -1;
                }

                var result = Comparer<int>.Default.Compare(ExcludingMinutes(x), ExcludingMinutes(y));
                if (result != 0)
                {
                    return result;
                }

                result = Comparer<int>.Default.Compare(x.MinutesDownloads.Length, y.MinutesDownloads.Length);
                if (result != 0)
                {
                    return -1 * result;
                }

                result = Comparer<int>.Default.Compare(DaysAndMinutesInSync(x), DaysAndMinutesInSync(y));
                if (result != 0)
                {
                    return result;
                }

                result = Comparer<DateTimeOffset>.Default.Compare(x.ExistingDays.Max, y.ExistingDays.Max);
                if (result != 0)
                {
                    return result;
                }

                return string.Compare(x.Symbol, y.Symbol, StringComparison.OrdinalIgnoreCase);

                int ExcludingMinutes(SymbolDownloads candidate)
                {
                    return this.settings.AlphaVantage.SymbolsWithMissingMinutes.Contains(candidate.Symbol) ? 1 : 0;
                }

                static int DaysAndMinutesInSync(SymbolDownloads x)
                {
                    return TradingDay.From(x.ExistingDays.Max) != TradingDay.From(x.ExistingMinutes.Max) ? 0 : 1;
                }
            }
        }

        public async Task<DaysAndSplits> DaysAndSplitsAsync(string symbol)
        {
            var download = DaysDownload.Create(symbol, default, this);
            await download.ExecuteAsync().ConfigureAwait(false);
            this.NewSymbol?.Invoke(this, symbol);

            return new DaysAndSplits(
                Database.ReadDays(symbol),
                Database.ReadSplits(symbol));
        }

        public void Add(Download download)
        {
            this.Downloads = this.Downloads.Add(download);
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.client?.Dispose();
        }

        public void Unlisted(string symbol)
        {
            this.settings.AlphaVantage.Unlisted(symbol);
            this.settings.Save();
        }

        public void MissingMinutes(string symbol)
        {
            this.settings.AlphaVantage.MissingMinutes(symbol);
            this.settings.Save();
        }

        public void FirstMinute(string symbol, DateTimeOffset first)
        {
            this.settings.AlphaVantage.FirstMinute(symbol, first);
            this.settings.Save();
        }

        public void NotifyDownloadedDays(string symbol) => this.NewDays?.Invoke(this, symbol);

        public void NotifyDownloadedMinutes(string symbol) => this.NewMinutes?.Invoke(this, symbol);

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
