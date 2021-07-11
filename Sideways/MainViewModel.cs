namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Media;
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
            this.Scan = new ScanViewModel(this.Downloader);
            this.Simulation = new SimulationViewModel(this);
            this.Animation = new AnimationViewModel(this);
            this.symbols = ImmutableSortedSet.CreateRange(Database.ReadSymbols());
            this.AddBookmarkCommand = new RelayCommand(_ => this.AddBookmark(this.Time));

            this.AddToWatchlistCommand = new RelayCommand(_ =>
            {
                switch (this)
                {
                    case { CurrentSymbol: { Symbol: { } symbol, Candles: { } }, WatchList: { } watchList }:
                        watchList.Add(symbol);
                        break;
                    case { WatchList: null }:
                        MessageBox.Show("No watchlist entry added, a watchlist must be open.", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                    default:
                        MessageBox.Show("No watchlist entry added.", MessageBoxButton.OK, MessageBoxImage.Error);
                        break;
                }
            });

            this.MoveToStartCommand = new RelayCommand(
                _ => this.Time = this.currentSymbol?.Candles?.FirstDay() ?? this.time,
                _ => this.currentSymbol is { Candles: { } });

            this.MoveToEndCommand = new RelayCommand(
                _ => this.Time = this.currentSymbol?.Candles?.LastDay() ?? this.time,
                _ => this.currentSymbol is { Candles: { } });

            this.SkipLeftCommand = new RelayCommand(
                o =>
                {
                    if (this is { currentSymbol: { Candles: { } candles } } &&
                        o is CandleInterval interval)
                    {
                        this.Time = candles.Skip(this.Time, interval, this.Animation.IsRunning ? -2 : -1);
                    }
                },
                o => this is { currentSymbol: { Candles: { } } } &&
                     o is CandleInterval);

            this.SkipRightCommand = new RelayCommand(
                o =>
                {
                    if (this is { currentSymbol: { Candles: { } candles } } &&
                        o is CandleInterval interval)
                    {
                        this.Time = candles.Skip(this.Time, interval, 1);
                    }
                },
                o => this is { currentSymbol: { Candles: { } } } &&
                     o is CandleInterval);

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
                    this.Time = (this.Bookmarks.Offset, this.currentSymbol?.Candles) switch
                    {
                        (var offset and not 0, { } candles) => candles.Skip(bookmark.Time, CandleInterval.Day, offset),
                        (var offset and not 0, null) => bookmark.Time.AddDays(offset),
                        _ => bookmark.Time,
                    };
                }
            };

            this.Scan.PropertyChanged += (_, e) =>
            {
                if (e is { PropertyName: nameof(ScanViewModel.SelectedResult) } &&
                    this.Scan.SelectedResult is { } bookmark)
                {
                    this.CurrentSymbol = SymbolViewModel.GetOrCreate(bookmark.Symbol, this.Downloader);
                    this.Time = (this.Scan.Offset, this.currentSymbol?.Candles) switch
                    {
                        (var offset and not 0, { } candles) => candles.Skip(bookmark.Time, CandleInterval.Day, offset),
                        (var offset and not 0, null) => bookmark.Time.AddDays(offset),
                        _ => bookmark.Time,
                    };
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

        public ScanViewModel Scan { get; }

        public SimulationViewModel Simulation { get; }

        public AnimationViewModel Animation { get; }

        public ICommand AddBookmarkCommand { get; }

        public ICommand AddToWatchlistCommand { get; }

        public ICommand MoveToStartCommand { get; }

        public ICommand MoveToEndCommand { get; }

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

        public void AddBookmark(DateTimeOffset time)
        {
            switch (this)
            {
                case { CurrentSymbol: { Symbol: { } symbol }, Bookmarks: { SelectedBookmarkFile: { } bookmarkFile } bookmarks }:
                    var bookmark = new Bookmark(symbol, time, ImmutableSortedSet<string>.Empty, null);
                    if (bookmarkFile.Bookmarks.Add(bookmark))
                    {
                        if (bookmark.Time == this.Time)
                        {
                            bookmarks.SelectedBookmark = bookmark;
                        }
                    }
                    else
                    {
                        SystemSounds.Beep.Play();
                    }

                    break;
                case { Bookmarks: { SelectedBookmarkFile: null } }:
                    MessageBox.Show("No bookmark added, a bookmarks file must be selected.",  MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case { CurrentSymbol: { Candles: { } } }:
                    MessageBox.Show("No bookmark added, a symbol with candles must be open.", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    MessageBox.Show("No bookmark added.", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
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
            this.Animation.Dispose();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
