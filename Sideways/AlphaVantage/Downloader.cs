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
        private AlphaVantageClient? client;

        private ImmutableList<Download> downloads = ImmutableList<Download>.Empty;
        private ImmutableList<TopUp> topUps = ImmutableList<TopUp>.Empty;
        private DownloadState topUpAllState = new();
        private bool disposed;

        public Downloader()
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.RefreshTopUpsCommand = new RelayCommand(_ => this.RefreshAsync());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.RunAllTopUpsCommand = new RelayCommand(_ => DownloadAllTopUps(), _ => !this.topUps.IsEmpty);

            async void DownloadAllTopUps()
            {
                this.TopUpAllState.Start = DateTimeOffset.Now;
                foreach (var topUp in this.topUps)
                {
                    if (topUp.DownloadCommand.CanExecute(null))
                    {
                        await topUp.DownloadAsync().ConfigureAwait(false);
                    }
                }

                this.topUpAllState.Exception = this.topUps.FirstOrDefault(x => x.DownloadState.Exception is { })?.DownloadState.Exception;
                this.TopUpAllState.End = DateTimeOffset.Now;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand RefreshTopUpsCommand { get; }

        public ICommand RunAllTopUpsCommand { get; }

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

        public ImmutableList<TopUp> TopUps
        {
            get => this.topUps;
            private set
            {
                if (ReferenceEquals(value, this.topUps))
                {
                    return;
                }

                this.topUps = value;
                this.OnPropertyChanged();
            }
        }

        public DownloadState TopUpAllState
        {
            get => this.topUpAllState;
            private set
            {
                if (value == this.topUpAllState)
                {
                    return;
                }

                this.topUpAllState = value;
                this.OnPropertyChanged();
            }
        }

        public async Task RefreshAsync()
        {
            this.TopUps = ImmutableList<TopUp>.Empty;
            this.TopUpAllState = new DownloadState();
            var dayRanges = await Task.Run(() => Database.DayRanges()).ConfigureAwait(false);
            var minuteRanges = await Task.Run(() => Database.MinuteRanges()).ConfigureAwait(false);
            this.TopUps = TopUps().OrderBy(x => x.LastComplete).ToImmutableList();

            IEnumerable<TopUp> TopUps()
            {
                foreach (var (symbol, dayRange) in dayRanges)
                {
                    if (minuteRanges.TryGetValue(symbol, out var minuteRange))
                    {
                        if (TradingDay.From(dayRange.Max) < TradingDay.LastComplete() ||
                            TradingDay.From(minuteRange.Max) < TradingDay.LastComplete())
                        {
                            yield return new TopUp(symbol, dayRange, minuteRange, this);
                        }
                    }
                }
            }
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
