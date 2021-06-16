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
        private bool disposed;

        public MainViewModel()
        {
            this.Settings = Settings.FromFile();
            this.Downloader = new(this.Settings);
            this.Simulation = new SimulationViewModel(this);
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
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Downloader Downloader { get; }

        public Settings Settings { get; }

        public BookmarksViewModel Bookmarks { get; } = new();

        public SimulationViewModel Simulation { get; }

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

        public ObservableCollection<string> WatchList { get; } = new();

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
