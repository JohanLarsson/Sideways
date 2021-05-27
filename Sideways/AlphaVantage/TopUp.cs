namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class TopUp
    {
        private readonly TimeRange existingDays;
        private readonly TimeRange existingMinutes;

        public TopUp(string symbol, TimeRange existingDays, TimeRange existingMinutes, Downloader downloader)
        {
            this.existingDays = existingDays;
            this.existingMinutes = existingMinutes;
            this.Symbol = symbol;
            if (TradingDay.From(existingDays.Max) < TradingDay.LastComplete())
            {
                this.DaysDownload = DaysDownload.Create(symbol, TradingDay.From(existingDays.Max), downloader);
            }

            if (TradingDay.From(existingMinutes.Max) < TradingDay.LastComplete())
            {
                this.MinutesDownloads = MinutesDownload.Create(symbol, existingDays, existingMinutes, downloader);
            }

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.DownloadCommand = new RelayCommand(_ => this.DownloadAsync(), _ => CanDownload());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            bool CanDownload()
            {
                return this switch
                {
                    { DaysDownload: { Start: { } } } => false,
                    { MinutesDownloads: { Length: > 0 } minutesDownloads } => minutesDownloads.All(x => x.Start is null),
                    _ => true,
                };
            }
        }

        public string Symbol { get; }

        public DaysDownload? DaysDownload { get; }

        public ImmutableArray<MinutesDownload>? MinutesDownloads { get; }

        public ICommand DownloadCommand { get; }

        public DownloadState DownloadState { get; } = new();

        public TradingDay LastDay => TradingDay.From(this.existingDays.Max);

        public TradingDay LastMinute => TradingDay.From(this.existingMinutes.Max);

        public TradingDay LastComplete => TradingDay.Min(this.LastDay, this.LastMinute);

        public async Task DownloadAsync()
        {
            this.DownloadState.Start = DateTimeOffset.Now;
            if (this.DaysDownload is { Start: null } daysDownload)
            {
                await daysDownload.ExecuteAsync().ConfigureAwait(false);
            }

            if (this.MinutesDownloads is { Length: > 0 } minutesDownloads)
            {
                foreach (var minutesDownload in minutesDownloads)
                {
                    if (minutesDownload.Start is null)
                    {
                        if (await minutesDownload.ExecuteAsync().ConfigureAwait(false) == 0)
                        {
                            return;
                        }
                    }
                }
            }

            this.DownloadState.Exception = this.DaysDownload?.Exception ??
                                           this.MinutesDownloads?.FirstOrDefault(x => x.Exception is { })?.Exception;
            this.DownloadState.End = DateTimeOffset.Now;
        }

        public override string ToString() => $"{this.Symbol} last day: {TradingDay.From(this.existingDays.Max)} last minute: {TradingDay.From(this.existingMinutes.Max)}";
    }
}
