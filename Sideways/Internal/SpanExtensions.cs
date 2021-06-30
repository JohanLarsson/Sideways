namespace Sideways
{
    using System;

    public static class SpanExtensions
    {
        public static float Adr(this ReadOnlySpan<Candle> candles) => 100 * (candles.Average(x => x.High / x.Low) - 1);

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
