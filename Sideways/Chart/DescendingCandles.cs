namespace Sideways
{
    using System.Collections;
    using System.Collections.Generic;

    public class DescendingCandles : IReadOnlyList<Candle>
    {
        private readonly List<Candle> candles = new();

        public int Count => this.candles.Count;

        public Candle this[int index] => this.candles[index];

        public IEnumerator<Candle> GetEnumerator() => this.candles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.candles).GetEnumerator();

        public void Clear() => this.candles.Clear();

        public void Add(Candle candle) => this.candles.Add(candle);
    }
}
