namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class DaysDownload : IDownload, INotifyPropertyChanged
    {
        private readonly Downloader downloader;
        private DateTimeOffset? start;
        private DateTimeOffset? end;
        private Exception? exception;

        public DaysDownload(string symbol, OutputSize outputSize, Downloader downloader)
        {
            this.downloader = downloader;
            this.Symbol = symbol;
            this.OutputSize = outputSize;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Symbol { get; }

        public OutputSize OutputSize { get; }

        public DateTimeOffset? Start
        {
            get => this.start;
            private set
            {
                if (value == this.start)
                {
                    return;
                }

                this.start = value;
                this.OnPropertyChanged();
            }
        }

        public DateTimeOffset? End
        {
            get => this.end;
            private set
            {
                if (value == this.end)
                {
                    return;
                }

                this.end = value;
                this.OnPropertyChanged();
            }
        }

        public Exception? Exception
        {
            get => this.exception;
            private set
            {
                if (ReferenceEquals(value, this.exception))
                {
                    return;
                }

                this.exception = value;
                this.OnPropertyChanged();
            }
        }

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
            this.Start = DateTimeOffset.Now;

            try
            {
                var candles = await this.downloader.Client.DailyAdjustedAsync(this.Symbol, this.OutputSize).ConfigureAwait(false);
                this.End = DateTimeOffset.Now;
                Database.WriteDays(this.Symbol, candles);
                return candles.Length;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                this.Exception = e;
                return 0;
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
