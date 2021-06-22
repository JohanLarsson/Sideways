namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows;
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
            this.Animation = new AnimationViewModel(this);
            this.symbols = ImmutableSortedSet.CreateRange(Database.ReadSymbols());
            this.AddBookmarkCommand = new RelayCommand(_ =>
            {
                switch (this)
                {
                    case { CurrentSymbol: { Symbol: { } symbol }, Time: { } time, Bookmarks: { SelectedBookmarkFile: { } bookmarkFile } }:
                        if (!bookmarkFile.Add(new Bookmark(symbol, time, ImmutableSortedSet<string>.Empty, null)))
                        {
                            _ = MessageBox.Show("Bookmark already exists.", "Bookmark", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                        break;
                    case { Bookmarks: { SelectedBookmarkFile: null } }:
                        MessageBox.Show("No bookmark added, a bookmarks file must be selected.", "Bookmark", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    case { CurrentSymbol: { Candles: { } } }:
                        MessageBox.Show("No bookmark added, a symbol with candles must be open.", "Bookmark", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show("No bookmark added.", "Bookmark", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            });

            this.AddToWatchlistCommand = new RelayCommand(_ =>
            {
                switch (this)
                {
                    case { CurrentSymbol: { Symbol: { } symbol, Candles: { } }, WatchList: { } watchList }:
                        watchList.Add(symbol);
                        break;
                    case { WatchList: null }:
                        MessageBox.Show("No watchlist entry added, a watchlist must be open.", "Bookmark", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show("No watchlist entry added.", "Watchlist", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            });

            this.SkipLeftCommand = new RelayCommand(o =>
            {
                if (this is { currentSymbol: { Candles: { } candles } } &&
                    o is CandleInterval interval)
                {
                    this.Time = candles.Skip(this.Time, interval, this.Animation.IsRunning ? -2 : -1);
                }
            });

            this.SkipRightCommand = new RelayCommand(o =>
            {
                if (this is { currentSymbol: { Candles: { } candles } } &&
                    o is CandleInterval interval)
                {
                    this.Time = candles.Skip(this.Time, interval, 1);
                }
            });

            _ = this.Downloader.RefreshSymbolDownloadsAsync();

            this.Downloader.NewSymbol += (_, symbol) => this.Symbols = this.symbols.Add(symbol);
            this.Downloader.NewDays += (_, symbol) => SymbolViewModel.Update(symbol);
            this.Downloader.NewMinutes += (_, symbol) => SymbolViewModel.Update(symbol);
            this.Downloader.NewEarnings += (_, symbol) => SymbolViewModel.Update(symbol);
            this.Bookmarks.PropertyChanged += (_, e) =>
            {
                if (e is { PropertyName: nameof(BookmarksViewModel.SelectedBookmark) } &&
                    this.Bookmarks.SelectedBookmark is { } bookmark)
                {
                    this.CurrentSymbol = SymbolViewModel.GetOrCreate(bookmark.Symbol, this.Downloader);
                    this.Time = this.Bookmarks.Offset == 0
                        ? bookmark.Time
                        : this.currentSymbol?.Candles is { } candles
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

        public AnimationViewModel Animation { get; }

        public ICommand AddBookmarkCommand { get; }

        public ICommand AddToWatchlistCommand { get; }

        public ICommand SkipLeftCommand { get; }

        public ICommand SkipRightCommand { get; }

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
            this.Animation.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
