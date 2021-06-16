namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    using Sideways.AlphaVantage;

    public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private ImmutableSortedSet<string> symbols;
        private DateTimeOffset time = DateTimeOffset.Now;
        private SymbolViewModel? currentSymbol;
        private Simulation? simulation;
        private bool disposed;

        public MainViewModel()
        {
            this.Settings = Settings.FromFile();
            this.Downloader = new(this.Settings);
            this.symbols = ImmutableSortedSet.CreateRange(Database.ReadSymbols());

            _ = this.Downloader.RefreshSymbolDownloadsAsync();

            this.Downloader.NewSymbol += (_, symbol) => this.Symbols = this.symbols.Add(symbol);
            this.Downloader.NewDays += (_, symbol) => SymbolViewModel.Update(symbol);
            this.Downloader.NewMinutes += (_, symbol) => SymbolViewModel.Update(symbol);
            this.Bookmarks.PropertyChanged += (_, e) =>
            {
                if (e is { PropertyName: nameof(BookmarksViewModel.SelectedBookmark) } &&
                    this.Bookmarks.SelectedBookmark is { } bookmark)
                {
                    this.CurrentSymbol = SymbolViewModel.GetOrCreate(bookmark.Symbol, this.Downloader);
                    this.Time = this.currentSymbol?.Candles is { } candles
                        ? candles.Skip(bookmark.Time, CandleInterval.Day, this.Bookmarks.Offset)
                        : bookmark.Time.AddDays(this.Bookmarks.Offset);
                }
            };
            this.WatchList.CollectionChanged += (_, _) =>
            {
                foreach (var symbol in this.WatchList)
                {
                    _ = SymbolViewModel.GetOrCreate(symbol, this.Downloader);
                }
            };

            this.currentSymbol = SymbolViewModel.GetOrCreate("TSLA", this.Downloader);
            this.BuyCommand = new RelayCommand(
                _ => Buy(),
                _ => this.Simulation is { Balance: > 1_000 } &&
                     this.currentSymbol is { Candles: { } });

            this.SellHalfCommand = new RelayCommand(
                _ => Sell(0.5),
                _ => this.simulation is { } &&
                     this.currentSymbol is { Candles: { } } symbol &&
                     this.simulation.Positions.Any(x => x.Symbol == symbol.Symbol));

            this.SellAllCommand = new RelayCommand(
                _ => Sell(1),
                _ => this.simulation is { } &&
                     this.currentSymbol is { Candles: { } } symbol &&
                     this.simulation.Positions.Any(x => x.Symbol == symbol.Symbol));

            void Buy()
            {
                var price = this.currentSymbol!.Candles!.Get(this.time, CandleInterval.Day).First().Close;
                var amount = Math.Min(this.simulation!.Balance, this.simulation.Equity() / 10);
                this.simulation.Buy(
                    this.currentSymbol.Symbol,
                    price,
                    (int)(amount / price),
                    this.time);
            }

            void Sell(double fraction)
            {
                var price = this.currentSymbol!.Candles!.Get(this.time, CandleInterval.Day).First().Close;
                this.simulation!.Sell(
                    this.currentSymbol.Symbol,
                    price,
                    (int)(fraction * this.simulation.Positions.Single(x => x.Symbol == this.currentSymbol.Symbol).Buys.Sum(x => x.Shares)),
                    this.time);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand BuyCommand { get; }

        public ICommand SellHalfCommand { get; }

        public ICommand SellAllCommand { get; }

        public Downloader Downloader { get; }

        public Settings Settings { get; }

        public ImmutableSortedSet<string> Symbols
        {
            get => this.symbols;
            private set
            {
                if (value == this.symbols)
                {
                    return;
                }

                this.symbols = value;
                this.OnPropertyChanged();
            }
        }

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
                this.CurrentSymbol = SymbolViewModel.GetOrCreate(value, this.Downloader);
                this.OnPropertyChanged();
            }
        }

        public Position? SelectedPosition
        {
            get => this.Simulation?.Positions.SingleOrDefault(x => x.Symbol == this.currentSymbol?.Symbol);
            set
            {
                if (value is { })
                {
                    this.CurrentSymbol = SymbolViewModel.GetOrCreate(value.Symbol, this.Downloader);
                }
            }
        }

        public ObservableCollection<string> WatchList { get; } = new();

        public BookmarksViewModel Bookmarks { get; } = new();

        public Simulation? Simulation
        {
            get => this.simulation;
            private set
            {
                if (ReferenceEquals(value, this.simulation))
                {
                    return;
                }

                this.simulation = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.SelectedPosition));
            }
        }

        public void UpdateSimulation(Simulation? fresh)
        {
            this.Simulation = fresh;
            if (fresh is { })
            {
                foreach (var position in fresh.Positions)
                {
                    _ = SymbolViewModel.GetOrCreate(position.Symbol, this.Downloader);
                }

                foreach (var position in fresh.Trades)
                {
                    _ = SymbolViewModel.GetOrCreate(position.Symbol, this.Downloader);
                }

                this.Time = fresh.Time;
            }
        }

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.Downloader.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
