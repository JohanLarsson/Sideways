namespace Sideways
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using Sideways.AlphaVantage;

    public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly string ApiKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways/AlphaVantage.key");
        private readonly Downloader downloader = new(new HttpClientHandler(), ApiKey());
        private readonly DataSource dataSource;
        private DateTimeOffset time = DateTimeOffset.Now;
        private SymbolViewModel? currentSymbol;
        private string selectedSymbol;
        private bool disposed;

        public MainViewModel()
        {
            this.dataSource = new DataSource(this.downloader);
            this.Symbols = new ReadOnlyObservableCollection<string>(new ObservableCollection<string>(Database.ReadSymbols()));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

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
            private set
            {
                if (ReferenceEquals(value, this.currentSymbol))
                {
                    return;
                }

                this.currentSymbol = value;
                this.OnPropertyChanged();
            }
        }

        public ReadOnlyObservableCollection<string> Symbols { get; }

        public string SelectedSymbol
        {
            get => this.selectedSymbol;
            set
            {
                if (value == this.selectedSymbol)
                {
                    return;
                }

                this.selectedSymbol = value;
                this.OnPropertyChanged();
                if (value is { })
                {
                    this.Load(value);
                }
                else
                {
                    this.CurrentSymbol = null;
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

        public void Load(string symbol)
        {
            this.ThrowIfDisposed();
            var symbolViewModel = new SymbolViewModel(symbol);
            this.CurrentSymbol = symbolViewModel;
            _ = symbolViewModel.LoadAsync(this.dataSource);
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
