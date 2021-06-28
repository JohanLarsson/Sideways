namespace Sideways
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Sideways.AlphaVantage;

    public sealed class SymbolViewModel : INotifyPropertyChanged
    {
        private static readonly ConcurrentDictionary<string, SymbolViewModel> Cache = new(StringComparer.OrdinalIgnoreCase);

        private Candles? candles;
        private ImmutableArray<QuarterlyEarning> quarterlyEarnings;
        private Exception? exception;

        private SymbolViewModel(string symbol)
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

        public ImmutableArray<QuarterlyEarning> QuarterlyEarnings
        {
            get => this.quarterlyEarnings;
            set
            {
                if (value == this.quarterlyEarnings)
                {
                    return;
                }

                this.quarterlyEarnings = value;
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

        public static SymbolViewModel? GetOrCreate(string? symbol, Downloader downloader)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return null;
            }

            return Cache.GetOrAdd(symbol, _ => Create());

            SymbolViewModel Create()
            {
                var vm = new SymbolViewModel(symbol.ToUpperInvariant());
                Load(vm);
                return vm;
            }

            async void Load(SymbolViewModel vm)
            {
                if (string.IsNullOrWhiteSpace(vm.Symbol))
                {
                    return;
                }

                try
                {
                    var splits = Database.ReadSplits(vm.Symbol);
                    var days = Database.ReadDays(vm.Symbol);
                    if (days.Count == 0)
                    {
                        var download = await downloader.DaysAndSplitsAsync(vm.Symbol).ConfigureAwait(true);
                        splits = download.Splits;
                        days = download.Candles;
                        vm.Candles = Candles.Adjusted(splits, days, default);
                    }
                    else
                    {
                        vm.Candles = Candles.Adjusted(splits, days, default);
                    }

                    vm.QuarterlyEarnings = Database.ReadQuarterlyEarnings(symbol);
                    var minutes = await Task.Run(() => Database.ReadMinutes(vm.Symbol)).ConfigureAwait(false);
                    vm.Candles = Candles.Adjusted(splits, days, minutes);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    vm.Exception = e;
                }
            }
        }

        public static void Update(string? symbol)
        {
            if (!string.IsNullOrWhiteSpace(symbol) &&
                Cache.TryGetValue(symbol, out var vm))
            {
                var splits = Database.ReadSplits(symbol);
                var days = Database.ReadDays(symbol);
                var minutes = Database.ReadMinutes(symbol);
                vm.Candles = Candles.Adjusted(splits, days, minutes);
                vm.QuarterlyEarnings = Database.ReadQuarterlyEarnings(symbol);
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
