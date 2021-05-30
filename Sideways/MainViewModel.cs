﻿namespace Sideways
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Sideways.AlphaVantage;

    public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly SymbolViewModelCache symbolViewModelCache;
        private ImmutableArray<string> symbols;
        private DateTimeOffset time = DateTimeOffset.Now;
        private SymbolViewModel? currentSymbol;
        private Simulation? simulation;
        private bool disposed;

        public MainViewModel()
        {
            this.Settings = Settings.FromFile();
            this.Downloader = new(this.Settings);
            this.symbolViewModelCache = new(this.Downloader);
            this.Symbols = Database.ReadSymbols();

            _ = this.Downloader.RefreshSymbolDownloadsAsync();

            this.Downloader.NewSymbol += (_, symbol) =>
            {
                if (InsertAt() is { } insertAt)
                {
                    this.Symbols = this.Symbols.Insert(insertAt, symbol);
                }
                else
                {
                    this.Symbols = this.Symbols.Add(symbol);
                }

                int? InsertAt()
                {
                    for (var i = 0; i < this.Symbols.Length; i++)
                    {
                        if (StringComparer.Ordinal.Compare(symbol, this.Symbols[i]) < 0)
                        {
                            return i;
                        }
                    }

                    return null;
                }
            };

            this.Downloader.NewDays += (_, symbol) => this.symbolViewModelCache.Update(symbol); ;
            this.Downloader.NewMinutes += (_, symbol) => this.symbolViewModelCache.Update(symbol); ;

            this.currentSymbol = this.symbolViewModelCache.Get("TSLA");
            this.BuyCommand = new RelayCommand(
                _ => Buy(),
                _ => this.Simulation is { Balance: > 1_000 } &&
                             this.currentSymbol is { Candles: { } });

            this.SellHalfCommand = new RelayCommand(
                _ => Sell(0.5),
                _ => this.Simulation is { } simulation &&
                     this.currentSymbol is { Candles: { } } symbol &&
                     simulation.Positions.Any(x => x.Symbol == symbol.Symbol));

            this.SellAllCommand = new RelayCommand(
                _ => Sell(1),
                _ => this.Simulation is { } simulation &&
                             this.currentSymbol is { Candles: { } } symbol &&
                             simulation.Positions.Any(x => x.Symbol == symbol.Symbol));

            this.StartNewSimulationCommand = new RelayCommand(_ => this.Simulation = Simulation.Create(this.Time));
            void Buy()
            {
                var price = this.currentSymbol!.Candles!.Get(this.time, CandleInterval.Day).First().Close;
                var amount = Math.Min(this.Simulation.Balance, this.Simulation.Equity() / 10);
                this.Simulation.Buy(
                    this.currentSymbol.Symbol,
                    price,
                    (int)(amount / price),
                    this.time);
            }

            void Sell(double fraction)
            {
                var price = this.currentSymbol!.Candles!.Get(this.time, CandleInterval.Day).First().Close;
                this.Simulation.Sell(
                    this.currentSymbol.Symbol,
                    price,
                    (int)(fraction * this.Simulation.Positions.Single(x => x.Symbol == this.currentSymbol.Symbol).Buys.Sum(x => x.Shares)),
                    this.time);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand BuyCommand { get; }

        public ICommand SellHalfCommand { get; }

        public ICommand SellAllCommand { get; }

        public ICommand StartNewSimulationCommand { get; }

        public Downloader Downloader { get; }

        public Settings Settings { get; }

        public ImmutableArray<string> Symbols
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
                this.CurrentSymbol = this.symbolViewModelCache.Get(value);
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
                    this.CurrentSymbol = this.symbolViewModelCache.Get(value.Symbol);
                }
            }
        }

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

        public void UpdateSimulation(Simulation? simulation)
        {
            this.Simulation = simulation;
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

                this.Time = simulation.Time;
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

        private sealed class SymbolViewModelCache
        {
            private readonly ConcurrentDictionary<string, SymbolViewModel?> symbolViewModels =
                new(StringComparer.OrdinalIgnoreCase);

            private readonly Downloader downloader;

            internal SymbolViewModelCache(Downloader downloader)
            {
                this.downloader = downloader;
            }

            internal SymbolViewModel? Get(string? symbol)
            {
                if (string.IsNullOrWhiteSpace(symbol))
                {
                    return null;
                }

                return this.symbolViewModels.GetOrAdd(symbol, x => Create(x));

                SymbolViewModel? Create(string symbol)
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
                            var download = await this.downloader.DaysAndSplitsAsync(vm.Symbol).ConfigureAwait(true);
                            splits = download.Splits;
                            days = download.Candles;
                            vm.Candles = Candles.Adjusted(splits, days, default);
                        }
                        else
                        {
                            vm.Candles = Candles.Adjusted(splits, days, default);
                        }

                        // Update minutes.
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

            internal void Update(string? symbol)
            {
                if (!string.IsNullOrWhiteSpace(symbol) &&
                    this.symbolViewModels.TryGetValue(symbol, out var vm) &&
                    vm is { })
                {
                    var splits = Database.ReadSplits(symbol);
                    var days = Database.ReadDays(symbol);
                    var minutes = Database.ReadMinutes(symbol);
                    vm.Candles = Candles.Adjusted(splits, days, minutes);
                }
            }
        }
    }
}
