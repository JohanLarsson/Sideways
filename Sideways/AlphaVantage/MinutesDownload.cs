namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class MinutesDownload : Download
    {
        private readonly Downloader downloader;

        public MinutesDownload(string symbol, Slice? slice, Downloader downloader)
            : base(symbol)
        {
            this.downloader = downloader;
            this.Slice = slice;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.DownloadCommand = new RelayCommand(_ => this.ExecuteAsync(), _ => this.State is { Status: DownloadStatus.Waiting or DownloadStatus.Error });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public Slice? Slice { get; }

        public ICommand DownloadCommand { get; }

        public TimeRange SliceRange => TimeRange.FromSlice(this.Slice ?? AlphaVantage.Slice.Year1Month1);

        public override string Info => $"Minutes from {this.SliceRange.Min:d} to {this.SliceRange.Max:d}";

        public static ImmutableArray<MinutesDownload> Create(string symbol, TimeRange existingDays, TimeRange existingMinutes, Downloader downloader, AlphaVantageSettings settings)
        {
            if (settings.SymbolsWithMissingMinutes.Contains(symbol))
            {
                return ImmutableArray<MinutesDownload>.Empty;
            }

            if (settings.UnlistedSymbols.Contains(symbol) &&
                existingDays.Max.Date == existingMinutes.Max.Date)
            {
                return ImmutableArray<MinutesDownload>.Empty;
            }

            if (TradingDay.From(existingMinutes.Max) < TradingDay.LastComplete() &&
                TradingDay.From(existingMinutes.Max.AddMonths(1)) >= TradingDay.LastComplete())
            {
                return ImmutableArray.Create(new MinutesDownload(symbol, null, downloader));
            }

            var builder = ImmutableArray.CreateBuilder<MinutesDownload>();
            foreach (var slice in Enum.GetValues<Slice>())
            {
                if (ShouldDownload(slice))
                {
                    builder.Add(new MinutesDownload(symbol, slice, downloader));
                }
            }

            return builder.ToImmutable();

            bool ShouldDownload(Slice slice)
            {
                var sliceRange = TimeRange.FromSlice(slice);
                if (existingMinutes.Contains(sliceRange))
                {
                    return false;
                }

                return true;
            }
        }

        public async Task<int> ExecuteAsync()
        {
            this.downloader.Add(this);
            this.State.Start = DateTimeOffset.Now;
            this.State.Exception = null;

            try
            {
                var candles = await Task().ConfigureAwait(false);
                this.State.End = DateTimeOffset.Now;
                if (!candles.IsDefaultOrEmpty)
                {
                    Database.WriteMinutes(this.Symbol, candles);
                    this.downloader.NotifyDownloadedMinutes(this.Symbol);
                }

                if (candles.IsDefaultOrEmpty && this.Slice is { } &&
                    Database.FirstMinute(this.Symbol) is { } first)
                {
                    this.downloader.FirstMinute(this.Symbol, first);
                    throw new InvalidOperationException("Downloaded empty slice, maybe missing data on AlphaVantage. Adding symbol to FirstMinutes in %APPDATA%\\Sideways\\Sideways.cfg");
                }

                if (candles.IsDefaultOrEmpty &&
                    IsMostRecentSlice())
                {
                    this.downloader.MissingMinutes(this.Symbol);
                    throw new InvalidOperationException("Downloaded empty slice, maybe missing data on AlphaVantage. Adding symbol to SymbolsWithMissingMinutes in %APPDATA%\\Sideways\\Sideways.cfg");
                }

                return candles.Length;

                bool IsMostRecentSlice()
                {
                    return this.Slice switch
                    {
                        AlphaVantage.Slice.Year1Month1 => true,
                        { } slice => TimeRange.FromSlice(slice).Contains(Database.DayRange(this.Symbol).Max),
                        null => true,
                    };
                }
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
