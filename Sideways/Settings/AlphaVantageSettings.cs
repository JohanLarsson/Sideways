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

        public AlphaVantageSettings(AlphaVantageClientSettings clientSettings, ImmutableSortedSet<string> symbolsWithMissingMinutes, ImmutableSortedSet<string> unlistedSymbols)
        {
            this.clientSettings = clientSettings;
            this.symbolsWithMissingMinutes = symbolsWithMissingMinutes;
            this.unlistedSymbols = unlistedSymbols;
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

        public void MissingMinutes(string symbol)
        {
            this.SymbolsWithMissingMinutes = this.symbolsWithMissingMinutes.Add(symbol);
        }

        public void Unlisted(string symbol)
        {
            this.UnlistedSymbols = this.unlistedSymbols.Add(symbol);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
