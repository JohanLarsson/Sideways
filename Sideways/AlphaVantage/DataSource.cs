namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    public sealed class DataSource : IDisposable
    {
        private readonly AlphaVantageClient client;
        private bool disposed;

        public DataSource(HttpMessageHandler messageHandler, string apiKey)
        {
            this.client = new AlphaVantageClient(messageHandler, apiKey);
        }

        public async Task<ImmutableArray<AdjustedCandle>> DaysAsync(string symbol)
        {
            var candles = await Database.ReadAdjustedDaysAsync(symbol).ConfigureAwait(false);
            if (candles.LastOrDefault().Time.Date == TradingDay.Last)
            {
                return candles;
            }

            if ((TradingDay.Last - candles.LastOrDefault().Time.Date).Days < 100)
            {
                Database.WriteAdjustedDays(symbol, await this.client.DailyAdjustedAsync(symbol, OutputSize.Compact).ConfigureAwait(false));
                return await Database.ReadAdjustedDaysAsync(symbol).ConfigureAwait(false);
            }

            var adjusted = await this.client.DailyAdjustedAsync(symbol, OutputSize.Full).ConfigureAwait(false);
            Database.WriteAdjustedDays(symbol, adjusted);
            return adjusted;
        }

        public async Task<ImmutableArray<Candle>> MinutesAsync(string symbol)
        {
            var candles = await Database.ReadMinutesAsync(symbol).ConfigureAwait(false);
            if (candles.Length > 0)
            {
                return candles;
            }

            var adjusted = await this.client.IntervalExtendedAsync(symbol, Interval.Minute, Slice.Year1Month1).ConfigureAwait(false);
            Database.WriteMinutes(symbol, adjusted);
            return await Database.ReadMinutesAsync(symbol).ConfigureAwait(false);
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
    }
}
