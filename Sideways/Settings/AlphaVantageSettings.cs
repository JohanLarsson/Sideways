namespace Sideways
{
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class AlphaVantageSettings : INotifyPropertyChanged
    {
        private AlphaVantageClientSettings clientSettings;
        private ImmutableSortedSet<string> symbolsWithMissingMinutes;
        private ImmutableSortedSet<string> unlistedSymbols;
        private ImmutableDictionary<string, TradingDay> firstDayWithMinutes;

        public AlphaVantageSettings(
            AlphaVantageClientSettings clientSettings,
            ImmutableSortedSet<string> symbolsWithMissingMinutes,
            ImmutableSortedSet<string> unlistedSymbols,
            ImmutableDictionary<string, TradingDay> firstDayWithMinutes)
        {
            this.clientSettings = clientSettings;
            this.symbolsWithMissingMinutes = symbolsWithMissingMinutes;
            this.unlistedSymbols = unlistedSymbols;
            this.firstDayWithMinutes = firstDayWithMinutes;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public AlphaVantageClientSettings ClientSettings
        {
            get => this.clientSettings;
            set
            {
                if (ReferenceEquals(value, this.clientSettings))
                {
                    return;
                }

                this.clientSettings = value;
                this.OnPropertyChanged();
            }
        }

        public ImmutableSortedSet<string> SymbolsWithMissingMinutes
        {
            get => this.symbolsWithMissingMinutes;
            private set
            {
                if (ReferenceEquals(value, this.symbolsWithMissingMinutes))
                {
                    return;
                }

                this.symbolsWithMissingMinutes = value;
                this.OnPropertyChanged();
            }
        }

        public ImmutableSortedSet<string> UnlistedSymbols
        {
            get => this.unlistedSymbols;
            private set
            {
                if (ReferenceEquals(value, this.unlistedSymbols))
                {
                    return;
                }

                this.unlistedSymbols = value;
                this.OnPropertyChanged();
            }
        }

        public ImmutableDictionary<string, TradingDay> FirstDayWithMinutes
        {
            get => this.firstDayWithMinutes;
            private set
            {
                if (ReferenceEquals(value, this.firstDayWithMinutes))
                {
                    return;
                }

                this.firstDayWithMinutes = value;
                this.OnPropertyChanged();
            }
        }

        public void Unlisted(string symbol)
        {
            this.UnlistedSymbols = this.unlistedSymbols.Add(symbol);
        }

        public void MissingMinutes(string symbol)
        {
            this.SymbolsWithMissingMinutes = this.symbolsWithMissingMinutes.Add(symbol);
        }

        public void HasMinutes(string symbol)
        {
            this.SymbolsWithMissingMinutes = this.symbolsWithMissingMinutes.Remove(symbol);
        }

        public void AddFirstDayWithMinutes(string symbol, TradingDay day)
        {
            this.FirstDayWithMinutes = this.firstDayWithMinutes.Add(symbol, day);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
