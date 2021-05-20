namespace Sideways
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    using Sideways.AlphaVantage;

    public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly SymbolViewModelCache symbolViewModelCache = new();

        private DateTimeOffset time = DateTimeOffset.Now;
        private SymbolViewModel? currentSymbol;
        private Simulation? simulation = Simulation.Create();
        private bool disposed;

        public MainViewModel()
        {
            this.Symbols = new ReadOnlyObservableCollection<string>(new ObservableCollection<string>(Database.ReadSymbols()));
            this.Listings = new ReadOnlyObservableCollection<Listing>(new ObservableCollection<Listing>(Database.ReadListings()));
            this.currentSymbol = this.symbolViewModelCache.Get("TSLA");
            this.BuyCommand = new RelayCommand(
                _ => Buy(),
                _ => this.simulation is { Balance: > 1_000 } &&
                             this.currentSymbol is { Candles: { } });

            this.SellHalfCommand = new RelayCommand(
                _ => Sell(0.5),
                _ => this.simulation is { } simulation &&
                     this.currentSymbol is { Candles: { } } symbol &&
                     simulation.Positions.Any(x => x.Symbol == symbol.Symbol));

            this.SellAllCommand = new RelayCommand(
                _ => Sell(1),
                _ => this.simulation is { } simulation &&
                             this.currentSymbol is { Candles: { } } symbol &&
                             simulation.Positions.Any(x => x.Symbol == symbol.Symbol));
            void Buy()
            {
                var price = this.currentSymbol!.Candles!.Get(this.time, CandleInterval.Day).First().Close;
                var amount = Math.Min(this.simulation.Balance, this.simulation.Equity() / 10);
                this.simulation.Buy(
                    this.currentSymbol.Symbol,
                    price,
                    (int)(amount / price),
                    this.time);
            }

            void Sell(double fraction)
            {
                var price = this.currentSymbol!.Candles!.Get(this.time, CandleInterval.Day).First().Close;
                this.simulation.Sell(
                    this.currentSymbol.Symbol,
                    price,
                    (int)fraction * this.simulation.Positions.Single(x => x.Symbol == this.currentSymbol.Symbol).Buys.Sum(x => x.Shares),
                    this.time);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ReadOnlyObservableCollection<string> Symbols { get; }

        public ReadOnlyObservableCollection<Listing> Listings { get; }

        public ICommand BuyCommand { get; }

        public ICommand SellHalfCommand { get; }

        public ICommand SellAllCommand { get; }

        public DateTimeOffset Time
        {
            get => this.time;
            set
            {
                if (value == this.time)
                {
                    return;
                }

                this.time = value;
                this.OnPropertyChanged();
            }
        }

        public SymbolViewModel? CurrentSymbol
        {
            get => this.currentSymbol;
            set
            {
                if (ReferenceEquals(value, this.currentSymbol))
                {
                    return;
                }

                this.currentSymbol = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.CurrentSymbolText));
                this.OnPropertyChanged(nameof(this.SelectedPosition));
            }
        }

        public string? CurrentSymbolText
        {
            get => this.currentSymbol?.Symbol;
            set
            {
                this.CurrentSymbol = this.symbolViewModelCache.Get(value);
                this.OnPropertyChanged();
            }
        }

        public Position? SelectedPosition
        {
            get => this.simulation?.Positions.SingleOrDefault(x => x.Symbol == this.currentSymbol?.Symbol);
            set
            {
                if (value is { })
                {
                    this.CurrentSymbol = this.symbolViewModelCache.Get(value.Symbol);
                }
            }
        }

        public Simulation? Simulation => this.simulation;

        public void UpdateSimulation(Simulation? simulation)
        {
            this.simulation = simulation;
            this.OnPropertyChanged(nameof(this.Simulation));
            this.OnPropertyChanged(nameof(this.SelectedPosition));
            if (simulation is { })
            {
                foreach (var position in simulation.Positions)
                {
                    _ = this.symbolViewModelCache.Get(position.Symbol);
                }

                foreach (var position in simulation.Trades)
                {
                    _ = this.symbolViewModelCache.Get(position.Symbol);
                }

                this.Time = simulation.Time ?? throw new InvalidOperationException("Missing time in simulation.");
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.symbolViewModelCache.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private sealed class SymbolViewModelCache : IDisposable
        {
            private static readonly string ApiKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways/AlphaVantage.key");

            private readonly ConcurrentDictionary<string, SymbolViewModel> symbolViewModels = new(StringComparer.OrdinalIgnoreCase);
            private readonly Downloader downloader = new(new HttpClientHandler(), ApiKey());
            private readonly DataSource dataSource;
            private bool disposed;

            internal SymbolViewModelCache()
            {
                this.dataSource = new DataSource(this.downloader);
            }

            public void Dispose()
            {
                if (this.disposed)
                {
                    return;
                }

                this.disposed = true;
                this.downloader.Dispose();
            }

            internal SymbolViewModel? Get(string? symbol)
            {
                if (symbol is null)
                {
                    return null;
                }

                return this.symbolViewModels.GetOrAdd(symbol, x => Create(x));

                SymbolViewModel Create(string symbol)
                {
                    var vm = new SymbolViewModel(symbol.ToUpperInvariant());
                    _ = vm.LoadAsync(this.dataSource);
                    return vm;
                }
            }

            private static string ApiKey()
            {
                if (File.Exists(ApiKeyFile))
                {
                    return File.ReadAllText(ApiKeyFile).Trim();
                }

                throw new InvalidOperationException($"Missing file {ApiKeyFile}");
            }
        }
    }
}
