namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Sideways.AlphaVantage;

    public sealed class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private static readonly string ApiKeyFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways/AlphaVantage.key");
        private readonly DataSource dataSource = new(new HttpClientHandler(), ApiKey());
        private DateTimeOffset endTime = DateTimeOffset.Now;
        private SymbolViewModel? currentSymbol;
        private bool disposed;

        public event PropertyChangedEventHandler? PropertyChanged;

        public DateTimeOffset EndTime
        {
            get => this.endTime;
            set
            {
                if (value == this.endTime)
                {
                    return;
                }

                this.endTime = value;
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

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.dataSource.Dispose();
        }

        public void Load(string symbol)
        {
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
