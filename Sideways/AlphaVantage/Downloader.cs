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
        private readonly AlphaVantageClient client = new(new HttpClientHandler(), AlphaVantageClient.ApiKey, 5);

        private ImmutableList<IDownload> downloads = ImmutableList<IDownload>.Empty;
        private bool disposed;

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

        public async Task<DaysAndSplits> DaysAndSplitsAsync(string symbol, TradingDay? from)
        {
            var download = DaysDownload.Create(symbol, from, this.client);
            this.Downloads = this.downloads.Add(download);
            Database.WriteDays(symbol, await download.Task().ConfigureAwait(false));

            return new DaysAndSplits(
                Database.ReadDays(symbol),
                Database.ReadSplits(symbol));
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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
