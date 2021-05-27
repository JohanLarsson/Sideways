namespace Sideways.AlphaVantage
{
    using System;
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
        private AlphaVantageClient? client;

        private ImmutableList<Download> downloads = ImmutableList<Download>.Empty;
        private ImmutableList<SymbolDownloads> symbolDownloads = ImmutableList<SymbolDownloads>.Empty;
        private DownloadState symbolDownloadState = new();
        private bool disposed;

        public Downloader()
        {
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

        public ICommand RefreshSymbolsCommand { get; }

        public ICommand DownloadAllSymbolsCommand { get; }

        public AlphaVantageClient Client => this.client ??= new(new HttpClientHandler(), AlphaVantageClient.ApiKey, 5);

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
            var dayRanges = await Task.Run(() => Database.DayRanges()).ConfigureAwait(false);
            var minuteRanges = await Task.Run(() => Database.MinuteRanges()).ConfigureAwait(false);
            this.SymbolDownloads = AlphaVantage.SymbolDownloads.Create(dayRanges, minuteRanges, this).OrderBy(x => x.LastComplete).ToImmutableList();
        }

        public async Task<DaysAndSplits> DaysAndSplitsAsync(string symbol, TradingDay? from)
        {
            var download = DaysDownload.Create(symbol, from, this);
            await download.ExecuteAsync().ConfigureAwait(false);

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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
