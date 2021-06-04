namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Net.Http;
    using System.Net.Http.Json;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;

    public sealed class AlphaVantageClient : IDisposable
    {
        private static readonly JsonSerializerOptions EarningsOptions = new(JsonSerializerDefaults.Web)
        {
            Converters =
            {
                AnnualEarningConverter.Default,
                QuarterlyEarningConverter.Default,
            },
        };

        private readonly string apiKey;
        private readonly HttpClient client;
        private readonly Throttle throttle;
        private bool disposed;

        public AlphaVantageClient(HttpMessageHandler messageHandler, string apiKey, int maxCallsPerMinute)
        {
            this.apiKey = apiKey;
            this.throttle = Throttle.GetOrCreate(apiKey, maxCallsPerMinute);
#pragma warning disable IDISP014 // Use a single instance of HttpClient.
            this.client = new HttpClient(messageHandler)
            {
                BaseAddress = new Uri("https://www.alphavantage.co", UriKind.Absolute),
            };
#pragma warning restore IDISP014 // Use a single instance of HttpClient.
        }

        public async Task<ImmutableArray<Listing>> ListingsAsync(CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            using (await this.throttle.WaitAsync().ConfigureAwait(false))
            {
                return await this.client.GetListingFromCsvAsync(
                    new Uri($"/query?function=LISTING_STATUS&apikey={this.apiKey}", UriKind.Relative),
                    cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<ImmutableArray<Candle>> WeeklyAsync(string symbol, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            using (await this.throttle.WaitAsync().ConfigureAwait(false))
            {
                return await this.client.GetCandlesFromCsvAsync(
                    new Uri($"/query?function=TIME_SERIES_WEEKLY&symbol={symbol}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                    cancellationToken).ConfigureAwait(false);
            }
        }

        public async Task<ImmutableArray<Candle>> DailyAsync(string symbol, OutputSize outputSize, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            using (await this.throttle.WaitAsync().ConfigureAwait(false))
            {
                return await this.client.GetCandlesFromCsvAsync(
                    new Uri($"/query?function=TIME_SERIES_DAILY&symbol={symbol}&outputsize={OutputSize()}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                    cancellationToken).ConfigureAwait(false);
            }

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
            using (await this.throttle.WaitAsync().ConfigureAwait(false))
            {
                return await this.client.GetAdjustedCandleFromCsvAsync(
                    new Uri($"/query?function=TIME_SERIES_DAILY_ADJUSTED&symbol={symbol}&outputsize={OutputSize()}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                    cancellationToken).ConfigureAwait(false);
            }

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
            using (await this.throttle.WaitAsync().ConfigureAwait(false))
            {
                return await this.client.GetCandlesFromCsvAsync(
                    new Uri($"query?function=TIME_SERIES_INTRADAY&symbol={symbol}&interval={Interval()}&adjusted={(adjusted ? "true" : "false")}&outputsize={OutputSize()}&datatype=csv&apikey={this.apiKey}", UriKind.Relative),
                    cancellationToken).ConfigureAwait(false);
            }

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
            using (await this.throttle.WaitAsync().ConfigureAwait(false))
            {
                return await this.client.GetCandlesFromCsvAsync(
#pragma warning disable CA1308 // Normalize strings to uppercase
                    new Uri($"query?function=TIME_SERIES_INTRADAY_EXTENDED&symbol={symbol}&interval={Interval()}&slice={slice.ToString().ToLowerInvariant()}&adjusted={(adjusted ? "true" : "false")}&apikey={this.apiKey}", UriKind.Relative),
#pragma warning restore CA1308 // Normalize strings to uppercase
                    cancellationToken).ConfigureAwait(false);
            }

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

        public async Task<Earnings> EarningsAsync(string symbol, CancellationToken cancellationToken = default)
        {
            this.ThrowIfDisposed();
            using (await this.throttle.WaitAsync().ConfigureAwait(false))
            {
                return await this.client.GetFromJsonAsync<Earnings>(
                    new Uri($"/query?function=EARNINGS&symbol={symbol}&apikey={this.apiKey}", UriKind.Relative),
                    EarningsOptions,
                    cancellationToken)
                                 .ConfigureAwait(false) ?? throw new InvalidOperationException("Earnings returned null.");
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
                throw new ObjectDisposedException(nameof(AlphaVantageClient));
            }
        }

        private class Throttle
        {
            private static readonly ConcurrentDictionary<string, Throttle> Singletons = new();

            private readonly SemaphoreSlim semaphore;

            private Throttle(int maxCallsPerMinute)
            {
                this.semaphore = new(maxCallsPerMinute);
            }

            internal static Throttle GetOrCreate(string apiKey, int maxCallsPerMinute) => Singletons.GetOrAdd(apiKey, _ => new Throttle(maxCallsPerMinute));

            internal async Task<IDisposable> WaitAsync()
            {
                await this.semaphore.WaitAsync().ConfigureAwait(false);
                return new Release(this.semaphore);
            }

            private sealed class Release : IDisposable
            {
                private readonly SemaphoreSlim semaphore;
                private bool disposed;

                internal Release(SemaphoreSlim semaphore)
                {
                    this.semaphore = semaphore;
                }

                public void Dispose()
                {
                    if (this.disposed)
                    {
                        return;
                    }

                    this.disposed = true;
                    _ = Task.Delay(TimeSpan.FromSeconds(60))
                        .ContinueWith(_ => this.semaphore.Release(), TaskScheduler.Default);
                }
            }
        }
    }
}
