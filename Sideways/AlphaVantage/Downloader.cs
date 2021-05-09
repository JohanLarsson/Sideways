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

        public async Task<Days> DaysAsync(string symbol, TradingDay? from)
        {
            this.ThrowIfDisposed();
            var download = Create(OutputSize());
            this.Downloads = this.downloads.Add(download);
            Database.WriteDays(symbol, await download.Task.ConfigureAwait(false));

            return new Days(
                Database.ReadDays(symbol),
                Database.ReadSplits(symbol),
                null);

            OutputSize OutputSize()
            {
                // Compact returns only last 100, below can be tweaked further as it includes holidays but good enough for now
                if (from is { Year: var y, Month: var m, Day: var d } &&
                    DateTime.Today - new DateTime(y, m, d) < TimeSpan.FromDays(100))
                {
                    return AlphaVantage.OutputSize.Compact;
                }

                return AlphaVantage.OutputSize.Full;
            }

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
