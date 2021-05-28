namespace Sideways
{
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class AlphaVantageSettings : INotifyPropertyChanged
    {
        private AlphaVantageClientSettings clientSettings = new();
        private ImmutableHashSet<string> symbolsWithMissingMinutes = ImmutableHashSet<string>.Empty;
        private ImmutableHashSet<string> unlistedSymbols = ImmutableHashSet<string>.Empty;

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

        public ImmutableHashSet<string> SymbolsWithMissingMinutes
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

        public ImmutableHashSet<string> UnlistedSymbols
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
