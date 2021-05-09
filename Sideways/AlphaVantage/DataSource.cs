namespace Sideways.AlphaVantage
{
    using System;
    using System.Linq;

    public sealed class DataSource
    {
        private readonly Downloader downloader;

        public DataSource(Downloader downloader)
        {
            this.downloader = downloader;
        }

        public Days Days(string symbol)
        {
            var candles = Database.ReadDays(symbol);
            var splits = Database.ReadSplits(symbol);
            var last = candles.IsDefaultOrEmpty ? (DateTimeOffset?)null : candles.Max(x => x.Time);
            if (last is { Date: var date } &&
                date == TradingDay.LastComplete.Date)
            {
                return new Days(candles, splits, null);
            }

            return new Days(candles, splits, this.downloader.DaysAsync(symbol, last));
        }

        //public async Task<ImmutableArray<Candle>> MinutesAsync(string symbol)
        //{
        //    var candles = await Database.ReadMinutesAsync(symbol).ConfigureAwait(false);
        //    if (candles.Length > 0)
        //    {
        //        return candles;
        //    }

        //    var adjusted = await this.client.IntervalExtendedAsync(symbol, Interval.Minute, Slice.Year1Month1, adjusted: false).ConfigureAwait(false);
        //    Database.WriteMinutes(symbol, adjusted);
        //    return await Database.ReadMinutesAsync(symbol).ConfigureAwait(false);
        //}
    }
}
