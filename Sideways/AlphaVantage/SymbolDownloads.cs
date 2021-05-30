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
        public SymbolDownloads(string symbol, TimeRange existingDays, DaysDownload? daysDownload, TimeRange existingMinutes, ImmutableArray<MinutesDownload> minutesDownloads)
        {
            this.Symbol = symbol;
            this.ExistingDays = existingDays;
            this.ExistingMinutes = existingMinutes;
            this.DaysDownload = daysDownload;
            this.MinutesDownloads = minutesDownloads;
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

        public TimeRange ExistingDays { get; }

        public TimeRange ExistingMinutes { get; }

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

        public TradingDay LastDay => TradingDay.From(this.ExistingDays.Max);

        public TradingDay LastMinute => TradingDay.From(this.ExistingMinutes.Max);

        public TradingDay LastComplete => this.MinutesDownloads.IsDefaultOrEmpty
            ? this.LastDay
            : TradingDay.Min(this.LastDay, this.LastMinute);

        public static SymbolDownloads? TryCreate(string symbol, TimeRange dayRange, TimeRange minuteRange, Downloader downloader, AlphaVantageSettings settings)
        {
            var daysDownload = DaysDownload.TryCreate(symbol, dayRange, downloader, settings);
            var minutesDownload = MinutesDownload.Create(symbol, dayRange, minuteRange, downloader, settings);

            if (daysDownload is null && minutesDownload.IsDefaultOrEmpty)
            {
                return null;
            }

            return new SymbolDownloads(symbol, dayRange, daysDownload, minuteRange, minutesDownload);
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

        public override string ToString() => $"{this.Symbol} last day: {TradingDay.From(this.ExistingDays.Max)} last minute: {TradingDay.From(this.ExistingMinutes.Max)}";
    }
}
