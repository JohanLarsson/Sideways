namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;

    public class MinutesDownload : Download
    {
        private readonly Downloader downloader;

        public MinutesDownload(string symbol, Slice? slice, Downloader downloader)
            : base(symbol)
        {
            this.downloader = downloader;
            this.Slice = slice;
        }

        public Slice? Slice { get; }

        public static ImmutableArray<MinutesDownload> Create(string symbol, TimeRange existingDays, TimeRange existingMinutes, Downloader down)
        {
            if (TradingDay.From(existingMinutes.Max.AddMonths(1)) >= TradingDay.LastComplete())
            {
                return ImmutableArray.Create(new MinutesDownload(symbol, null, down));
            }

            return ImmutableArray<MinutesDownload>.Empty;
        }

        public async Task<int> ExecuteAsync()
        {
            this.downloader.Add(this);
            this.State.Start = DateTimeOffset.Now;

            try
            {
                var candles = await Task().ConfigureAwait(false);
                this.State.End = DateTimeOffset.Now;
                Database.WriteMinutes(this.Symbol, candles);
                return candles.Length;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                this.State.Exception = e;
                this.State.End = DateTimeOffset.Now;
                return 0;
            }

            Task<ImmutableArray<Candle>> Task()
            {
                if (this.Slice is { } slice)
                {
                    return this.downloader.Client.IntradayExtendedAsync(this.Symbol, Interval.Minute, slice);
                }
                else
                {
                    return this.downloader.Client.IntradayAsync(this.Symbol, Interval.Minute);
                }
            }
        }
    }
}
