namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class MinutesDownload : IDownload, INotifyPropertyChanged
    {
        private readonly AlphaVantageClient client;
        private Task<ImmutableArray<Candle>>? task;
        private DateTimeOffset? started;

        public MinutesDownload(string symbol, Slice? slice, AlphaVantageClient client)
        {
            this.client = client;
            this.Symbol = symbol;
            this.Slice = slice;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Symbol { get; }

        public Slice? Slice { get; }

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

        public static ImmutableArray<MinutesDownload> Create(string symbol, TimeRange existingDays, TimeRange existingMinutes, AlphaVantageClient client)
        {
            if (TradingDay.From(existingMinutes.Max.AddMonths(1)) >= TradingDay.LastComplete())
            {
                return ImmutableArray.Create(new MinutesDownload(symbol, null, client));
            }

            return ImmutableArray<MinutesDownload>.Empty;
        }

#pragma warning disable VSTHRD200 // Use "Async" suffix for async methods
        public Task<ImmutableArray<Candle>> Task()
#pragma warning restore VSTHRD200 // Use "Async" suffix for async methods
        {
            if (this.task == null)
            {
                this.Started = DateTimeOffset.Now;
                if (this.Slice is { } slice)
                {
                    this.task = this.client.IntradayExtendedAsync(this.Symbol, Interval.Minute, slice);
                }
                else
                {
                    this.task = this.client.IntradayAsync(this.Symbol, Interval.Minute);
                }
            }

            return this.task;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
