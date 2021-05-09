namespace Sideways
{
    using System;

    public class CandleBuilder
    {
        public DateTimeOffset? Time;
        private float open;
        private float high = float.MinValue;
        private float low = float.MaxValue;
        private float close;
        private int volume;

        public void Add(Candle candle)
        {
            if (candle.Time > this.Time)
            {
                throw new InvalidOperationException("Expected earlier candle.");
            }

            if (this.Time is null)
            {
                this.close = candle.Close;
            }

            this.Time = candle.Time;
            this.high = Math.Max(this.high, candle.High);
            this.low = Math.Min(this.low, candle.Low);
            this.open = candle.Open;
            this.volume += candle.Volume;
        }

        public Candle Build()
        {
            var candle = new Candle(
                time: this.Time ?? throw new InvalidOperationException("Empty builder."),
                open: this.open,
                high: this.high,
                low: this.low,
                close: this.close,
                volume: this.volume);
            this.Time = null;
            this.open = 0;
            this.high = float.MinValue;
            this.low = float.MaxValue;
            this.close = 0;
            this.volume = 0;
            return candle;
        }
    }
}
