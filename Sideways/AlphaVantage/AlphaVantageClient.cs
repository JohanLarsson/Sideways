namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class AlphaVantageClient : IDisposable
    {
        public static readonly string ApiKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways/AlphaVantage.key");

        private readonly string apiKey;
        private readonly HttpClient client;
        private readonly Throttle throttle;
        private bool disposed;

        public AlphaVantageClient(HttpMessageHandler messageHandler, string apiKey, int maxCallsPerMinute)
        {
            this.apiKey = apiKey;
            this.throttle = new Throttle(maxCallsPerMinute);
#pragma warning disable IDISP014 // Use a single instance of HttpClient.
            this.client = new HttpClient(messageHandler)
            {
                BaseAddress = new Uri("https://www.alphavantage.co", UriKind.Absolute),
            };
#pragma warning restore IDISP014 // Use a single instance of HttpClient.
        }

        public static string ApiKey
        {
            get
            {
                if (File.Exists(ApiKeyFile))
                {
                    return File.ReadAllText(ApiKeyFile).Trim();
                }

                throw new InvalidOperationException($"Missing file {ApiKeyFile}");
            }

            set => File.WriteAllText(ApiKeyFile, value.Trim());
        }

        public async Task<ImmutableArray<Listing>> ListingsAsync(CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            await this.throttle.WaitAsync().ConfigureAwait(false);
            return await this.client.GetListingFromCsvAsync(
                new Uri($"/query?function=LISTING_STATUS&apikey={this.apiKey}", UriKind.Relative),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<ImmutableArray<Candle>> WeeklyAsync(string symbol, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            await this.throttle.WaitAsync().ConfigureAwait(false);
            return await this.client.GetCandlesFromCsvAsync(
                new Uri($"/query?function=TIME_SERIES_WEEKLY&symbol={symbol}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                cancellationToken).ConfigureAwait(false);
        }

        public async Task<ImmutableArray<Candle>> DailyAsync(string symbol, OutputSize outputSize, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            await this.throttle.WaitAsync().ConfigureAwait(false);
            return await this.client.GetCandlesFromCsvAsync(
                new Uri($"/query?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize={OutputSize()}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                cancellationToken).ConfigureAwait(false);
            string OutputSize()
            {
                return outputSize switch
                {
                    AlphaVantage.OutputSize.Full => "full",
                    AlphaVantage.OutputSize.Compact => "compact",
                    _ => throw new ArgumentOutOfRangeException(nameof(outputSize), outputSize, null),
                };
            }
        }

        public async Task<ImmutableArray<AdjustedCandle>> DailyAdjustedAsync(string symbol, OutputSize outputSize, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            await this.throttle.WaitAsync().ConfigureAwait(false);
            return await this.client.GetAdjustedCandleFromCsvAsync(
                new Uri($"/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol={symbol}&outputsize={OutputSize()}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                cancellationToken).ConfigureAwait(false);

            string OutputSize()
            {
                return outputSize switch
                {
                    AlphaVantage.OutputSize.Full => "full",
                    AlphaVantage.OutputSize.Compact => "compact",
                    _ => throw new ArgumentOutOfRangeException(nameof(outputSize), outputSize, null),
                };
            }
        }

        public async Task<ImmutableArray<Candle>> IntradayAsync(string symbol, Interval interval, bool adjusted = false, OutputSize outputSize = OutputSize.Full, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            await this.throttle.WaitAsync().ConfigureAwait(false);
            return await this.client.GetCandlesFromCsvAsync(
                new Uri($"query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval={Interval()}&adjusted={(adjusted ? "true" : "false")}&outputsize={OutputSize()}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                cancellationToken).ConfigureAwait(false);

            string Interval() => interval switch
            {
                AlphaVantage.Interval.Minute => "1min",
                AlphaVantage.Interval.FiveMinutes => "5min",
                AlphaVantage.Interval.FifteenMinutes => "15min",
                AlphaVantage.Interval.ThirtyMinutes => "30min",
                AlphaVantage.Interval.Hour => "60min",
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null),
            };

            string OutputSize() => outputSize switch
            {
                AlphaVantage.OutputSize.Full => "full",
                AlphaVantage.OutputSize.Compact => "compact",
                _ => throw new ArgumentOutOfRangeException(nameof(outputSize), outputSize, null),
            };
        }

        public async Task<ImmutableArray<Candle>> IntradayExtendedAsync(string symbol, Interval interval, Slice slice, bool adjusted = false, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            await this.throttle.WaitAsync().ConfigureAwait(false);
            return await this.client.GetCandlesFromCsvAsync(
#pragma warning disable CA1308 // Normalize strings to uppercase
                new Uri($"query?function=TIME_SERIES_INTRADAY_EXTENDED&symbol={symbol}&interval={Interval()}&slice={slice.ToString().ToLowerInvariant()}&adjusted={(adjusted ? "true" : "false")}&apikey={this.apiKey}", UriKind.Relative),
#pragma warning restore CA1308 // Normalize strings to uppercase
                cancellationToken).ConfigureAwait(false);

            string Interval() => interval switch
            {
                AlphaVantage.Interval.Minute => "1min",
                AlphaVantage.Interval.FiveMinutes => "5min",
                AlphaVantage.Interval.FifteenMinutes => "15min",
                AlphaVantage.Interval.ThirtyMinutes => "30min",
                AlphaVantage.Interval.Hour => "60min",
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null),
            };
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
                throw new ObjectDisposedException(nameof(AlphaVantageClient));
            }
        }

        private class Throttle
        {
            private readonly SemaphoreSlim semaphore;

            internal Throttle(int maxCallsPerMinute)
            {
                this.semaphore = new(maxCallsPerMinute - 1);
            }

            internal Task WaitAsync()
            {
                _ = Task.Delay(TimeSpan.FromSeconds(60))
                        .ContinueWith(_ => this.semaphore.Release(), TaskScheduler.Default);
                return this.semaphore.WaitAsync();
            }
        }
    }
}
