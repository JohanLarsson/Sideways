namespace Sideways
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
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
        private bool disposed;

        public MainViewModel()
        {
            this.dataSource = new DataSource(this.downloader);
            this.Symbols = new ReadOnlyObservableCollection<SymbolViewModel>(new ObservableCollection<SymbolViewModel>(Database.ReadSymbols().Select(x => Create(x))));

            SymbolViewModel Create(string symbol)
            {
                var vm = new SymbolViewModel(symbol);
                _ = vm.LoadAsync(this.dataSource);
                return vm;
            }
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
            set
            {
                if (ReferenceEquals(value, this.currentSymbol))
                {
                    return;
                }

                this.currentSymbol = value;
                this.OnPropertyChanged();
            }
        }

        public ReadOnlyObservableCollection<SymbolViewModel> Symbols { get; }

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
