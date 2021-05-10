namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Sideways.AlphaVantage;

    public sealed class SymbolViewModel : INotifyPropertyChanged
    {
        private SortedCandles? days;
        private SortedCandles? minutes;
        private Exception? exception;

        public SymbolViewModel(string symbol)
        {
            this.Symbol = symbol;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Symbol { get; }

        public SortedCandles? Days
        {
            get => this.days;
            set
            {
                if (ReferenceEquals(value, this.days))
                {
                    return;
                }

                this.days = value;
                this.OnPropertyChanged();
            }
        }

        public SortedCandles? Minutes
        {
            get => this.minutes;
            set
            {
                if (ReferenceEquals(value, this.minutes))
                {
                    return;
                }

                this.minutes = value;
                this.OnPropertyChanged();
            }
        }

        public Exception? Exception
        {
            get => this.exception;
            set
            {
                if (ReferenceEquals(value, this.exception))
                {
                    return;
                }

                this.exception = value;
                this.OnPropertyChanged();
            }
        }

        public async Task LoadAsync(DataSource dataSource)
        {
            try
            {
                var days = dataSource.Days(this.Symbol);
                this.Days = new SortedCandles(days.Candles);
                //this.Minutes = new SortedCandles(await dataSource.MinutesAsync(this.Symbol).ConfigureAwait(false));
                if (days.Download is { } daysDownload)
                {
                    days = await daysDownload.ConfigureAwait(false);
                    this.Days = new SortedCandles(days.Candles);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                this.Exception = e;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
