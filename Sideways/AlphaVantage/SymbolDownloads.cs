namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class SymbolDownloads
    {
        private readonly TimeRange existingDays;
        private readonly TimeRange existingMinutes;

        public SymbolDownloads(string symbol, TimeRange existingDays, TimeRange existingMinutes, Downloader downloader)
        {
            this.existingDays = existingDays;
            this.existingMinutes = existingMinutes;
            this.Symbol = symbol;
            if (TradingDay.From(existingDays.Max) < TradingDay.LastComplete())
            {
                this.DaysDownload = DaysDownload.Create(symbol, TradingDay.From(existingDays.Max), downloader);
            }

            this.MinutesDownloads = MinutesDownload.Create(symbol, existingDays, existingMinutes, downloader);

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.DownloadCommand = new RelayCommand(_ => this.DownloadAsync(), _ => CanDownload());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

            bool CanDownload()
            {
                return this switch
                {
                    { DaysDownload: { State: { Start: { } } } } => false,
                    { MinutesDownloads: { Length: > 0 } minutesDownloads } => minutesDownloads.All(x => x is { State: { Start: null } }),
                    _ => true,
                };
            }
        }

        public string Symbol { get; }

        public DaysDownload? DaysDownload { get; }

        public ImmutableArray<MinutesDownload> MinutesDownloads { get; }

        public IEnumerable<Download> AllDownloads
        {
            get
            {
                if (this.DaysDownload is { } daysDownload)
                {
                    yield return daysDownload;
                }

                foreach (var minutesDownload in this.MinutesDownloads)
                {
                    yield return minutesDownload;
                }
            }
        }

        public ICommand DownloadCommand { get; }

        public DownloadState State { get; } = new();

        public TradingDay LastDay => TradingDay.From(this.existingDays.Max);

        public TradingDay LastMinute => TradingDay.From(this.existingMinutes.Max);

        public TradingDay LastComplete => TradingDay.Min(this.LastDay, this.LastMinute);

        public static IEnumerable<SymbolDownloads> Create(ImmutableDictionary<string, TimeRange> dayRanges, ImmutableDictionary<string, TimeRange> minuteRanges, Downloader downloader, AlphaVantageSettings settings)
        {
            foreach (var (symbol, dayRange) in dayRanges)
            {
                if (settings.UnlistedSymbols.Contains(symbol))
                {
                    continue;
                }

                if (minuteRanges.TryGetValue(symbol, out var minuteRange) &&
                    !settings.SymbolsWithMissingMinutes.Contains(symbol))
                {
                    if (TradingDay.From(dayRange.Max) < TradingDay.LastComplete() ||
                        TradingDay.From(minuteRange.Max) < TradingDay.LastComplete())
                    {
                        yield return new SymbolDownloads(symbol, dayRange, minuteRange, downloader);
                    }
                }
                else if (TradingDay.From(dayRange.Max) < TradingDay.LastComplete())
                {
                    yield return new SymbolDownloads(symbol, dayRange, default, downloader);
                }
            }
        }

        public async Task DownloadAsync()
        {
            this.State.Start = DateTimeOffset.Now;
            if (this.DaysDownload is { State: { Start: null } } daysDownload)
            {
                await daysDownload.ExecuteAsync().ConfigureAwait(false);
            }

            if (this.MinutesDownloads is { Length: > 0 } minutesDownloads)
            {
                foreach (var minutesDownload in minutesDownloads)
                {
                    if (minutesDownload is { State: { Start: null } })
                    {
                        if (await minutesDownload.ExecuteAsync().ConfigureAwait(false) == 0)
                        {
                            break;
                        }
                    }
                }
            }

            this.State.Exception = this.DaysDownload?.State.Exception ??
                                   this.MinutesDownloads.FirstOrDefault(x => x.State.Exception is { })?.State.Exception;
            this.State.End = DateTimeOffset.Now;
        }

        public override string ToString() => $"{this.Symbol} last day: {TradingDay.From(this.existingDays.Max)} last minute: {TradingDay.From(this.existingMinutes.Max)}";
    }
}
