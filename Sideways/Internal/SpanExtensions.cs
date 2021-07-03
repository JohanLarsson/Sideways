namespace Sideways
{
    using System;

    public static class SpanExtensions
    {
        public static Percent Adr(this ReadOnlySpan<Candle> candles) => new(candles.Average(x => Percent.Change(x.Low, x.High).Scalar));

        public static float Atr(this ReadOnlySpan<Candle> candles)
        {
            if (candles.IsEmpty)
            {
                return 0;
            }

            if (candles.Length == 1)
            {
                return candles[0].High - candles[0].Low;
            }

            var sum = 0f;
            if (candles[0].Time < candles[1].Time)
            {
                for (var i = 1; i < candles.Length; i++)
                {
                    sum += TrueRange(candles[i], candles[i - 1]);
                }
            }
            else
            {
                for (var i = 0; i < candles.Length - 1; i++)
                {
                    sum += TrueRange(candles[i], candles[i + 1]);
                }
            }

            return sum / (candles.Length - 1);

            static float TrueRange(Candle c, Candle p)
            {
                // https://www.investopedia.com/terms/a/atr.asp
                var h = c.High;
                var l = c.Low;
                var cp = p.Close;
                return Math.Max(h - l, Math.Max(Math.Abs(h - cp), Math.Abs(l - cp)));
            }
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
    }
}
