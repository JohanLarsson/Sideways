namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;

    using Sideways.AlphaVantage;

    public sealed class SymbolViewModel : INotifyPropertyChanged
    {
        private Candles? candles;
        private Exception? exception;

        public SymbolViewModel(string symbol)
        {
            this.Symbol = symbol;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Symbol { get; }

        public Candles? Candles
        {
            get => this.candles;
            set
            {
                if (ReferenceEquals(value, this.candles))
                {
                    return;
                }

                this.candles = value;
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

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
