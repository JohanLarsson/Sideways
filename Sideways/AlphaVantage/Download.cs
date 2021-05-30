namespace Sideways.AlphaVantage
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class Download : INotifyPropertyChanged
    {
        protected Download(string symbol)
        {
            this.Symbol = symbol;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Symbol { get; }

        public DownloadState State { get; } = new();

        public abstract string Info { get; }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
