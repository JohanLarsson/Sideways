namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    public class SortedCandles : IEnumerable<Candle>
    {
        private readonly ImmutableArray<Candle> candles;

        public SortedCandles(IEnumerable<Candle> raw)
        {
            this.candles = raw.OrderByDescending(x => x.Time).ToImmutableArray();
        }

        public IEnumerable<Candle> Get(DateTimeOffset start)
        {
            return this.candles.SkipWhile(x => x.Time > start);
        }

        public Candle? Previous(DateTimeOffset time)
        {
            foreach (var candle in this.candles)
            {
                if (candle.Time < time)
                {
                    return candle;
                }
            }

            return null;
        }

        public Candle? Next(DateTimeOffset time)
        {
            for (var i = this.candles.Length - 1; i >= 0; i--)
            {
                var candle = this.candles[i];
                if (candle.Time > time)
                {
                    return candle;
                }
            }

            return null;
        }

        public IEnumerator<Candle> GetEnumerator() => ((IEnumerable<Candle>)this.candles).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.candles).GetEnumerator();
    }
}
