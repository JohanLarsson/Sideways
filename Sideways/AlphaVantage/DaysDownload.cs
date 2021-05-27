namespace Sideways.AlphaVantage
{
    using System;
    using System.Threading.Tasks;

    public class DaysDownload : Download
    {
        private readonly Downloader downloader;

        public DaysDownload(string symbol, OutputSize outputSize, Downloader downloader)
            : base(symbol)
        {
            this.downloader = downloader;
            this.OutputSize = outputSize;
        }

        public OutputSize OutputSize { get; }

        public static DaysDownload Create(string symbol, TradingDay? from, Downloader downloader)
        {
            return new DaysDownload(symbol, OutputSize(), downloader);

            OutputSize OutputSize()
            {
                // Compact returns only last 100, below can be tweaked further as it includes holidays but good enough for now
                if (from is { Year: var y, Month: var m, Day: var d } &&
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

            try
            {
                var candles = await this.downloader.Client.DailyAdjustedAsync(this.Symbol, this.OutputSize).ConfigureAwait(false);
                this.State.End = DateTimeOffset.Now;
                Database.WriteDays(this.Symbol, candles);
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
