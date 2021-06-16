namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Windows;
    using System.Windows.Input;

    using Microsoft.Win32;

    public sealed class SimulationViewModel : INotifyPropertyChanged
    {
        public static readonly string Directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Simulations");

        private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

        private readonly MainViewModel main;

        private Simulation? current;
        private FileInfo? file;

        public SimulationViewModel(MainViewModel main)
        {
            this.main = main;
            main.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(main.CurrentSymbol))
                {
                    this.OnPropertyChanged(nameof(this.SelectedPosition));
                }
            };

            this.BuyCommand = new RelayCommand(
                _ => Buy(),
                _ => this.current is { Balance: > 1_000 } &&
                     main.CurrentSymbol is { Candles: { } });

            this.SellHalfCommand = new RelayCommand(
                _ => Sell(0.5),
                _ => this.current is { Positions: { Count: > 0 } positions } &&
                     main.CurrentSymbol is { Candles: { } } symbol &&
                     positions.Any(x => x.Symbol == symbol.Symbol));

            this.SellAllCommand = new RelayCommand(
                _ => Sell(1),
                _ => this.current is { Positions: { Count: > 0 } positions } &&
                     main.CurrentSymbol is { Candles: { } } symbol &&
                     positions.Any(x => x.Symbol == symbol.Symbol));

            void Buy()
            {
                if (main is { CurrentSymbol: { Symbol: { } symbol, Candles: { } candles } } &&
                    this.Current is { } simulation)
                {
                    var price = candles.Get(main.Time, CandleInterval.Day).First().Close;
                    var amount = Math.Min(simulation.Balance, simulation.Equity() / 10);
                    simulation.Buy(
                        symbol,
                        price,
                        (int)(amount / price),
                        main.Time);
                }
                else
                {
                    _ = MessageBox.Show("Cannot buy symbol.", "Simulation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            void Sell(double fraction)
            {
                if (main is { CurrentSymbol: { Symbol: { } symbol, Candles: { } candles } } &&
                    this.Current is { } simulation)
                {
                    var price = candles.Get(main.Time, CandleInterval.Day).First().Close;
                    simulation.Sell(
                        symbol,
                        price,
                        (int)(fraction * simulation.Positions.Single(x => x.Symbol == symbol).Buys.Sum(x => x.Shares)),
                        main.Time);
                }
                else
                {
                    _ = MessageBox.Show("Cannot sell symbol.", "Simulation", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand BuyCommand { get; }

        public ICommand SellHalfCommand { get; }

        public ICommand SellAllCommand { get; }

        public Simulation? Current
        {
            get => this.current;
            private set
            {
                if (ReferenceEquals(value, this.current))
                {
                    return;
                }

                this.current = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.SelectedPosition));
            }
        }

        public Position? SelectedPosition
        {
            get => this.Current?.Positions.SingleOrDefault(x => x.Symbol == this.main.CurrentSymbol?.Symbol);
            set
            {
                if (value is { })
                {
                    this.main.CurrentSymbol = SymbolViewModel.GetOrCreate(value.Symbol, this.main.Downloader);
                }
            }
        }

        public void StartNew()
        {
            this.AskSave();
            this.UpdateSimulation(Simulation.Create(this.main.Time));
        }

        public void Load(FileInfo fileInfo)
        {
            this.AskSave();
            this.UpdateSimulation(JsonSerializer.Deserialize<Simulation>(File.ReadAllText(fileInfo.FullName)));
            this.file = fileInfo;
        }

        public void Close()
        {
            this.AskSave();
            this.Current = null;
            this.file = null;
        }

        public void AskSave()
        {
            if (this.current is { } simulation &&
                IsDirty() &&
                MessageBox.Show("Do you want to save current simulation first?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.Save();
            }

            bool IsDirty()
            {
                if (this.file is null)
                {
                    return true;
                }

                return this.file is { FullName: { } fileName } &&
                       JsonSerializer.Serialize(simulation, JsonSerializerOptions) != File.ReadAllText(fileName);
            }
        }

        public void Save()
        {
            if (this.current is { } simulation)
            {
                if (!System.IO.Directory.Exists(Directory))
                {
                    System.IO.Directory.CreateDirectory(Directory);
                }

                if (File() is { FullName: { } fullName })
                {
                    simulation.Time = this.main.Time;
                    System.IO.File.WriteAllText(fullName, JsonSerializer.Serialize(simulation, JsonSerializerOptions));
                }

                FileInfo? File()
                {
                    if (this.file is { } file)
                    {
                        return file;
                    }

                    var dialog = new SaveFileDialog
                    {
                        InitialDirectory = Directory,
                        FileName = DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                        DefaultExt = ".simulation",
                        Filter = "Simulation files|*.simulation;*.sim",
                    };

                    if (dialog.ShowDialog() is true)
                    {
                        this.file = new FileInfo(dialog.FileName);
                        return new(dialog.FileName);
                    }

                    return null;
                }
            }
        }

        private void UpdateSimulation(Simulation? simulation)
        {
            this.Current = simulation;
            if (simulation is { })
            {
                foreach (var position in simulation.Positions)
                {
                    _ = SymbolViewModel.GetOrCreate(position.Symbol, this.main.Downloader);
                }

                foreach (var position in simulation.Trades)
                {
                    _ = SymbolViewModel.GetOrCreate(position.Symbol, this.main.Downloader);
                }

                this.main.Time = simulation.Time;
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
