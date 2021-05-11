namespace Sideways.AlphaVantage
{
    public sealed class DataSource
    {
        private readonly Downloader downloader;

        public DataSource(Downloader downloader)
        {
            this.downloader = downloader;
        }

        public Days Days(string symbol)
        {
            var descendingDays = Database.ReadDays(symbol);
            var splits = Database.ReadSplits(symbol);
            var last = descendingDays.Count == 0 ? (TradingDay?)null : TradingDay.Create(descendingDays[0].Time);
            if (last == TradingDay.LastComplete())
            {
                return new Days(descendingDays, splits, null);
            }

            return new Days(descendingDays, splits, this.downloader.DaysAsync(symbol, last));
        }

        //public Minutes Minutes(string symbol)
        //{
        //    var candles = Database.ReadMinutes(symbol);
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
