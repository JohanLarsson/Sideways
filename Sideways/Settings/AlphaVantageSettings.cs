namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class AlphaVantageSettings : INotifyPropertyChanged
    {
        private AlphaVantageClientSettings clientSettings;
        private ImmutableSortedSet<string> symbolsWithMissingMinutes;
        private ImmutableSortedSet<string> unlistedSymbols;
        private ImmutableSortedDictionary<string, DateTimeOffset> firstMinutes;

        public AlphaVantageSettings(
            AlphaVantageClientSettings clientSettings,
            ImmutableSortedSet<string> symbolsWithMissingMinutes,
            ImmutableSortedSet<string> unlistedSymbols,
            ImmutableSortedDictionary<string, DateTimeOffset> firstMinutes)
        {
            this.clientSettings = clientSettings;
            //// ReSharper disable ConstantNullCoalescingCondition
            this.symbolsWithMissingMinutes = symbolsWithMissingMinutes ?? ImmutableSortedSet<string>.Empty;
            this.unlistedSymbols = unlistedSymbols ?? ImmutableSortedSet<string>.Empty;
            this.firstMinutes = firstMinutes ?? ImmutableSortedDictionary<string, DateTimeOffset>.Empty;
            //// ReSharper restore ConstantNullCoalescingCondition
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

        public ImmutableSortedDictionary<string, DateTimeOffset> FirstMinutes
        {
            get => this.firstMinutes;
            private set
            {
                if (ReferenceEquals(value, this.firstMinutes))
                {
                    return;
                }

                this.firstMinutes = value;
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

        public void FirstMinute(string symbol, DateTimeOffset first)
        {
            this.FirstMinutes = this.firstMinutes.Add(symbol, first);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
