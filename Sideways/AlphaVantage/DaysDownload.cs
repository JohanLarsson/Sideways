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

        public Task<ImmutableArray<AdjustedCandle>> Task()
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
