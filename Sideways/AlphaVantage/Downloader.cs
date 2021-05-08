namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public sealed class Downloader : IDisposable, INotifyPropertyChanged
    {
        private readonly AlphaVantageClient client;
        private bool disposed;
        private ImmutableList<IDownload> downloads = ImmutableList<IDownload>.Empty;

        public Downloader(HttpMessageHandler messageHandler, string apiKey)
        {
            this.client = new AlphaVantageClient(messageHandler, apiKey);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ImmutableList<IDownload> Downloads
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

        public async Task<Days> DaysAsync(string symbol, DateTimeOffset? from)
        {
            this.ThrowIfDisposed();
            var download = from is { } &&
                           TradingDay.LastComplete - from < TimeSpan.FromDays(100)
                ? Create(OutputSize.Compact)
                : Create(OutputSize.Full);
            this.Downloads = this.downloads.Add(download);
            Database.WriteDays(symbol, await download.Task.ConfigureAwait(false));

            return new Days(
                await Database.ReadDaysAsync(symbol).ConfigureAwait(false),
                await Database.ReadSplitsAsync(symbol).ConfigureAwait(false),
                null);

            DaysDownload Create(OutputSize size)
            {
                return new(symbol, size, this.client.DailyAdjustedAsync(symbol, size));
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.client.Dispose();
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(Downloader));
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
