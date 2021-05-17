namespace Sideways
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    using Sideways.AlphaVantage;

    public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly string ApiKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways/AlphaVantage.key");
        private readonly Downloader downloader = new(new HttpClientHandler(), ApiKey());
        private readonly DataSource dataSource;
        private DateTimeOffset time = DateTimeOffset.Now;
        private SymbolViewModel? currentSymbol;
        private Simulation simulation = new();
        private bool disposed;

        public MainViewModel()
        {
            this.dataSource = new DataSource(this.downloader);
            this.Symbols = new ReadOnlyObservableCollection<SymbolViewModel>(new ObservableCollection<SymbolViewModel>(Database.ReadSymbols().Select(x => new SymbolViewModel(x))));
            _ = Task.WhenAll(this.Symbols.Select(x => x.LoadAsync(this.dataSource)));
            this.BuyCommand = new RelayCommand(
                _ => Buy(),
                _ => this.simulation is { Balance: > 10_000 } &&
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
                var price = this.currentSymbol.Candles!.Get(this.time, CandleInterval.Day).First().Close;
                var amount = Math.Min(this.simulation.Balance, this.simulation.Equity() / 10);
                this.simulation.Buy(
                    this.currentSymbol.Symbol,
                    price,
                    (int)(amount / price),
                    this.time);
            }

            void Sell(double fraction)
            {
                var price = this.currentSymbol.Candles!.Get(this.time, CandleInterval.Day).First().Close;
                this.simulation.Sell(
                    this.currentSymbol.Symbol,
                    price,
                    (int)fraction * this.simulation.Positions.Single(x => x.Symbol == this.currentSymbol.Symbol).Buys.Sum(x => x.Shares),
                    this.time);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ReadOnlyObservableCollection<SymbolViewModel> Symbols { get; }

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
                this.OnPropertyChanged(nameof(this.SelectedPosition));
            }
        }

        public Simulation Simulation
        {
            get => this.simulation;
            set
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

        public Position? SelectedPosition
        {
            get => this.simulation.Positions.SingleOrDefault(x => x.Symbol == this.currentSymbol?.Symbol);
            set
            {
                if (value is { })
                {
                    this.CurrentSymbol = this.Symbols.SingleOrDefault(x => x.Symbol == value.Symbol);
                    this.OnPropertyChanged();
                }
            }
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

        private static string ApiKey()
        {
            if (File.Exists(ApiKeyFile))
            {
                return File.ReadAllText(ApiKeyFile).Trim();
            }

            throw new InvalidOperationException($"Missing file {ApiKeyFile}");
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new ObjectDisposedException(nameof(MainViewModel));
            }
        }
    }
}
