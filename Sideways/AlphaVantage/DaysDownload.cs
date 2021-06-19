namespace Sideways.AlphaVantage
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class DaysDownload : Download
    {
        private readonly Downloader downloader;

        public DaysDownload(string symbol, TimeRange existingDays, OutputSize outputSize, Downloader downloader)
            : base(symbol)
        {
            this.downloader = downloader;
            this.ExistingDays = existingDays;
            this.OutputSize = outputSize;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.DownloadCommand = new RelayCommand(_ => this.ExecuteAsync(), _ => this.State is { Status: DownloadStatus.Waiting or DownloadStatus.Error });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public TimeRange ExistingDays { get; }

        public OutputSize OutputSize { get; }

        public ICommand DownloadCommand { get; }

        public override string Info => $"Days from {this.ExistingDays.Max:d}";

        public static DaysDownload? TryCreate(string symbol, TimeRange dayRange, Downloader downloader, AlphaVantageSettings settings)
        {
            if (TradingDay.From(dayRange.Max) < TradingDay.LastComplete() &&
                !settings.UnlistedSymbols.Contains(symbol))
            {
                return Create(symbol, dayRange, downloader);
            }

            return null;
        }

        public static DaysDownload Create(string symbol, TimeRange dayRange, Downloader downloader)
        {
            return new DaysDownload(symbol, dayRange, OutputSize(), downloader);

            OutputSize OutputSize()
            {
                // Compact returns only last 100, below can be tweaked further as it includes holidays but good enough for now
                if (dayRange.Max is { Year: var y, Month: var m, Day: var d } &&
                    DateTime.Today - new DateTime(y, m, d) < TimeSpan.FromDays(100))
                {
                    return AlphaVantage.OutputSize.Compact;
                }

                return AlphaVantage.OutputSize.Full;
            }
        }

        public async Task<int> ExecuteAsync()
        {
            this.downloader.Add(this);
            this.State.Start = DateTimeOffset.Now;
            this.State.Exception = null;

            try
            {
                var candles = await this.downloader.Client.DailyAdjustedAsync(this.Symbol, this.OutputSize).ConfigureAwait(false);
                if (TradingDay.From(candles.Max(x => x.Time).AddDays(5)) < TradingDay.LastComplete())
                {
                    this.downloader.Unlisted(this.Symbol);
                }

                this.State.End = DateTimeOffset.Now;
                if (!candles.IsDefaultOrEmpty)
                {
                    Database.WriteDays(this.Symbol, candles);
                    this.downloader.NotifyDownloadedDays(this.Symbol);
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
        }
    }
}
