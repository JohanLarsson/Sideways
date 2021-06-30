namespace Sideways
{
    using System;

    public static class SpanExtensions
    {
        public static float Adr(this ReadOnlySpan<Candle> candles) => 100 * (candles.Average(x => x.High / x.Low) - 1);

        public static float Atr(this ReadOnlySpan<Candle> candles)
        {
            var sum = 0f;
            for (var i = 0; i < candles.Length - 1; i++)
            {
                // https://www.investopedia.com/terms/a/atr.asp
                var h = candles[i].High;
                var l = candles[i].Low;
                var cp = candles.Previous(i).Close;
                sum += Math.Max(h - l, Math.Max(Math.Abs(h - cp), Math.Abs(l - cp)));
            }

            return sum / (candles.Length - 1);
        }

        public static float Average(this ReadOnlySpan<Candle> span, Func<Candle, float> selector)
        {
            var sum = 0f;
            foreach (var c in span)
            {
                sum += selector(c);
            }

            return sum / span.Length;
        }

        private static Candle Previous(this ReadOnlySpan<Candle> candles, int index)
        {
            if (index > 0 &&
                candles[index - 1].Time < candles[index].Time)
            {
                return candles[index - 1];
            }

            if (index < candles.Length - 1 &&
                candles[index + 1].Time < candles[index].Time)
            {
                return candles[index + 1];
            }

            throw new InvalidOperationException("Did not find previous candle.");
        }
    }
}
