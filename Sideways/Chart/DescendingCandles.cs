namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class DescendingCandles : IReadOnlyList<Candle>, INotifyPropertyChanged
    {
        private readonly List<Candle> candles = new();

        private int visibleCount;
        private int extraCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Count => this.candles.Count;

        public Candle? FirstVisible => this.candles.Count == 0 ? null : this.candles[Math.Min(this.candles.Count - 1, this.VisibleCount)];

        public Candle? LastVisible => this.candles.Count == 0 ? null : this.candles[0];

        public int VisibleCount
        {
            get => this.visibleCount;
            set
            {
                if (value == this.visibleCount)
                {
                    return;
                }

                this.visibleCount = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.FirstVisible));
            }
        }

        public int ExtraCount
        {
            get => this.extraCount;
            set
            {
                if (value == this.extraCount)
                {
                    return;
                }

                this.extraCount = value;
                this.OnPropertyChanged();
            }
        }

        public Candle this[int index] => this.candles[index];

        // ReSharper disable once UnusedMember.Global
        public ReadOnlySpan<Candle> Slice(int start, int length) => CollectionsMarshal.AsSpan(this.candles).Slice(start, length);

        public Percent? Adr(int start = 0)
        {
            if (this.CanSlice(start, 20))
            {
                return this.Slice(start, 20).Adr();
            }

            return null;
        }

        public float? Atr(int start = 0)
        {
            if (this.CanSlice(start, 21))
            {
                return this.Slice(start, 21).Atr();
            }

            return null;
        }

        public IEnumerator<Candle> GetEnumerator() => this.candles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.candles).GetEnumerator();

        public void Clear() => this.candles.Clear();

        public void Add(Candle candle) => this.candles.Add(candle);

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool CanSlice(int start, int length) => this.candles.Count > start + length;
    }
}
