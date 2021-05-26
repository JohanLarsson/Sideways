namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class DaysDownload : IDownload, INotifyPropertyChanged
    {
        private readonly AlphaVantageClient client;
        private Task<ImmutableArray<AdjustedCandle>>? task;
        private DateTimeOffset? started;

        public DaysDownload(string symbol, OutputSize outputSize, AlphaVantageClient client)
        {
            this.client = client;
            this.Symbol = symbol;
            this.OutputSize = outputSize;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Symbol { get; }

        public OutputSize OutputSize { get; }

        public DateTimeOffset? Started
        {
            get => this.started;
            private set
            {
                if (value == this.started)
                {
                    return;
                }

                this.started = value;
                this.OnPropertyChanged();
            }
        }

        public static DaysDownload Create(string symbol, TradingDay? from, AlphaVantageClient client)
        {
            return new DaysDownload(symbol, OutputSize(), client);

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

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public Task<ImmutableArray<AdjustedCandle>> Task()
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            if (this.task == null)
            {
                this.Started = DateTimeOffset.Now;
                this.task = this.client.DailyAdjustedAsync(this.Symbol, this.OutputSize);
            }

            return this.task;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
