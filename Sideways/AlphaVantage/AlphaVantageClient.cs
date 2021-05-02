namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class AlphaVantageClient : IDisposable
    {
        private readonly string apiKey;
        private readonly HttpClient client;
        private bool disposed;

        public AlphaVantageClient(HttpMessageHandler messageHandler, string apiKey)
        {
            this.apiKey = apiKey;
#pragma warning disable IDISP014 // Use a single instance of HttpClient.
            this.client = new HttpClient(messageHandler)
            {
                BaseAddress = new Uri("https://www.alphavantage.co", UriKind.Absolute),
            };
#pragma warning restore IDISP014 // Use a single instance of HttpClient.
        }

        public Task<ImmutableArray<Candle>> WeeklyAsync(string symbol, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            return this.client.GetCandlesFromCsvAsync(
                new Uri($"/query?function=TIME_SERIES_WEEKLY&symbol={symbol}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                cancellationToken);
        }

        public Task<ImmutableArray<Candle>> DailyAsync(string symbol, OutputSize outputSize, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            return this.client.GetCandlesFromCsvAsync(
                new Uri($"/query?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize={OutputSize()}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                cancellationToken);
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

        public Task<ImmutableArray<AdjustedCandle>> DailyAdjustedAsync(string symbol, OutputSize outputSize, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            return this.client.GetAdjustedCandleFromCsvAsync(
                new Uri($"/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol={symbol}&outputsize={OutputSize()}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                cancellationToken);

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

        public Task<ImmutableArray<Candle>> IntervalAsync(string symbol, Interval interval, OutputSize outputSize, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            return this.client.GetCandlesFromCsvAsync(
                new Uri($"query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval={Interval()}&outputsize={OutputSize()}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                cancellationToken);

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

        public Task<ImmutableArray<Candle>> IntervalExtendedAsync(string symbol, Interval interval, Slice slice, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();

            return this.client.GetCandlesFromCsvAsync(
#pragma warning disable CA1308 // Normalize strings to uppercase
                new Uri($"query?function=TIME_SERIES_INTRADAY_EXTENDED&symbol={symbol}&interval={Interval()}&slice={slice.ToString().ToLowerInvariant()}&apikey={this.apiKey}", UriKind.Relative),
#pragma warning restore CA1308 // Normalize strings to uppercase
                cancellationToken);

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
    }
}
