namespace Sideways.Scan
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class Filter : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual int ExtraDays { get; }

        public abstract string Info { get; }

        public abstract bool IsMatch(SortedCandles candles, int index);

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
