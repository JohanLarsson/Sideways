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

        public static ImmutableArray<MinutesDownload> Create(string symbol, TimeRange existingDays, TimeRange existingMinutes, Downloader downloader)
        {
            if (existingMinutes == default)
            {
                var builder = ImmutableArray.CreateBuilder<MinutesDownload>();
                foreach (var slice in Enum.GetValues<Slice>())
                {
                    if (existingDays.Overlaps(TimeRange.FromSlice(slice)))
                    {
                        builder.Add(new MinutesDownload(symbol, slice, downloader));
                    }
                }

                return builder.ToImmutable();
            }

            if (TradingDay.From(existingMinutes.Max.AddMonths(1)) >= TradingDay.LastComplete())
            {
                return ImmutableArray.Create(new MinutesDownload(symbol, null, downloader));
            }

            if (TradingDay.From(existingMinutes.Min) > TradingDay.Max(TradingDay.From(existingDays.Min), TradingDay.From(TimeRange.FromSlice(AlphaVantage.Slice.Year2Month2).Min)))
            {
                var builder = ImmutableArray.CreateBuilder<MinutesDownload>();
                foreach (var slice in Enum.GetValues<Slice>())
                {
                    if (existingDays.Overlaps(TimeRange.FromSlice(slice)) &&
                        !existingMinutes.Contains(TimeRange.FromSlice(slice)))
                    {
                        builder.Add(new MinutesDownload(symbol, slice, downloader));
                    }
                }

                return builder.ToImmutable();
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
                if (candles.IsDefaultOrEmpty &&
                    this.Slice is null or AlphaVantage.Slice.Year1Month1)
                {
                    this.downloader.MissingMinutes(this.Symbol);
                    throw new InvalidOperationException("Downloaded empty slice, maybe missing data on AlphaVantage. Exclude this symbol?");
                }

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

                return this.downloader.Client.IntradayAsync(this.Symbol, Interval.Minute);
            }
        }
    }
}
