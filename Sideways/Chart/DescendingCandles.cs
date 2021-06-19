namespace Sideways
{
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class DescendingCandles : IReadOnlyList<Candle>, INotifyPropertyChanged
    {
        private readonly List<Candle> candles = new();
        private int extraCandles;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Count => this.candles.Count;

        public int VisibleCandles { get; set; }

        public int ExtraCandles
        {
            get => this.extraCandles;
            set
            {
                if (value == this.extraCandles)
                {
                    return;
                }

                this.extraCandles = value;
                this.OnPropertyChanged();
            }
        }

        public Candle this[int index] => this.candles[index];

        public IEnumerator<Candle> GetEnumerator() => this.candles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.candles).GetEnumerator();

        public void Clear() => this.candles.Clear();

        public void Add(Candle candle) => this.candles.Add(candle);

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
