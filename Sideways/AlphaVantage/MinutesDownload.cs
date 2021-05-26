namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    public class MinutesDownload : IDownload, INotifyPropertyChanged
    {
        private readonly Downloader downloader;
        private DateTimeOffset? start;
        private DateTimeOffset? end;
        private Exception? exception;

        public MinutesDownload(string symbol, Slice? slice, Downloader downloader)
        {
            this.downloader = downloader;
            this.Symbol = symbol;
            this.Slice = slice;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Symbol { get; }

        public Slice? Slice { get; }

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

        public static ImmutableArray<MinutesDownload> Create(string symbol, TimeRange existingDays, TimeRange existingMinutes, Downloader down)
        {
            if (TradingDay.From(existingMinutes.Max.AddMonths(1)) >= TradingDay.LastComplete())
            {
                return ImmutableArray.Create(new MinutesDownload(symbol, null, down));
            }

            return ImmutableArray<MinutesDownload>.Empty;
        }

        public async Task<int> ExecuteAsync()
        {
            this.downloader.Add(this);
            this.Start = DateTimeOffset.Now;

            try
            {
                var candles = await Task().ConfigureAwait(false);
                this.End = DateTimeOffset.Now;
                Database.WriteMinutes(this.Symbol, candles);
                return candles.Length;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                this.Exception = e;
                return 0;
            }

            Task<ImmutableArray<Candle>> Task()
            {
                if (this.Slice is { } slice)
                {
                    return this.downloader.Client.IntradayExtendedAsync(this.Symbol, Interval.Minute, slice);
                }
                else
                {
                    return this.downloader.Client.IntradayAsync(this.Symbol, Interval.Minute);
                }
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
