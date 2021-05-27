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

    public sealed class Downloader : IDisposable, INotifyPropertyChanged
    {
        private AlphaVantageClient? client;

        private ImmutableList<Download> downloads = ImmutableList<Download>.Empty;
        private ImmutableList<TopUp> topUps = ImmutableList<TopUp>.Empty;
        private bool disposed;

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public AlphaVantageClient Client => this.client ??= new(new HttpClientHandler(), AlphaVantageClient.ApiKey, 5);

        public async Task RefreshAsync()
        {
            this.TopUps = ImmutableList<TopUp>.Empty;
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
