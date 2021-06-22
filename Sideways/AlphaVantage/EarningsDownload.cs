namespace Sideways.AlphaVantage
{
    using System;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class EarningsDownload : Download
    {
        private readonly Downloader downloader;

        public EarningsDownload(string symbol, Downloader downloader)
            : base(symbol)
        {
            this.downloader = downloader;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            this.DownloadCommand = new RelayCommand(_ => this.ExecuteAsync(), _ => this.State is { Status: DownloadStatus.Waiting or DownloadStatus.Error });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        }

        public ICommand DownloadCommand { get; }

        public override string Info => "Earnings";

        public static EarningsDownload? TryCreate(string symbol, Downloader downloader, AlphaVantageSettings settings)
        {
            if (ShouldDownload())
            {
                return new EarningsDownload(symbol, downloader);
            }

            return null;

            bool ShouldDownload()
            {
                return Database.LastEarnings(symbol) switch
                {
                    null => true,
                    { } date => !settings.UnlistedSymbols.Contains(symbol) && (DateTimeOffset.Now - date).Days > 100,
                };
            }
        }

        public async Task<int> ExecuteAsync()
        {
            this.downloader.Add(this);
            this.State.Start = DateTimeOffset.Now;
            this.State.Exception = null;

            try
            {
                var earnings = await this.downloader.Client.EarningsAsync(this.Symbol).ConfigureAwait(false);

                this.State.End = DateTimeOffset.Now;
                if (!earnings.QuarterlyEarnings.IsDefaultOrEmpty)
                {
                    Database.WriteQuarterlyEarnings(earnings.Symbol, earnings.QuarterlyEarnings);
                }

                if (!earnings.AnnualEarnings.IsDefaultOrEmpty)
                {
                    Database.WriteAnnualEarnings(earnings.Symbol, earnings.AnnualEarnings);
                }

                if (earnings is { AnnualEarnings: { IsDefaultOrEmpty: false } } or { QuarterlyEarnings: { IsDefaultOrEmpty: false } })
                {
                    this.downloader.NotifyDownloadedEarnings(this.Symbol);
                }

                return earnings.QuarterlyEarnings.Length;
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
