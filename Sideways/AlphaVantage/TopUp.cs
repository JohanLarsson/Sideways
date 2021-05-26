namespace Sideways.AlphaVantage
{
    using System.Collections.Immutable;

    public class TopUp
    {
        private readonly TimeRange existingDays;
        private readonly TimeRange existingMinutes;

        public TopUp(string symbol, TimeRange existingDays, TimeRange existingMinutes, AlphaVantageClient client)
        {
            this.existingDays = existingDays;
            this.existingMinutes = existingMinutes;
            this.Symbol = symbol;
            if (TradingDay.From(existingDays.Max) < TradingDay.LastComplete())
            {
                this.DaysDownload = DaysDownload.Create(symbol, TradingDay.From(existingDays.Max), client);
            }

            if (TradingDay.From(existingMinutes.Max) < TradingDay.LastComplete())
            {
                this.MinutesDownloads = MinutesDownload.Create(symbol, existingDays, existingMinutes, client);
            }
        }

        public string Symbol { get; }

        public DaysDownload? DaysDownload { get; }

        public ImmutableArray<MinutesDownload>? MinutesDownloads { get; }

        public TradingDay LastDay => TradingDay.From(this.existingDays.Max);

        public TradingDay LastMinute => TradingDay.From(this.existingMinutes.Max);

        public TradingDay LastComplete => TradingDay.Min(this.LastDay, this.LastMinute);

        public override string ToString() => $"{this.Symbol} last day: {TradingDay.From(this.existingDays.Max)} last minute: {TradingDay.From(this.existingMinutes.Max)}";
    }
}
