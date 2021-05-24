namespace Sideways.Tests.Model
{
    using System;
    using NUnit.Framework;

    public static class SplitsTests
    {
        [Test]
        public static void Adjust()
        {
            var splitBuilder = SortedSplits.CreateBuilder();
            splitBuilder.Add(new Split(new DateTimeOffset(2020, 08, 31, 0, 0, 0, 0, TimeSpan.Zero), 5));
            var splits = splitBuilder.Create();

            var candles = SortedCandles.Create(
                new Candle(new DateTimeOffset(2020, 8, 26, 0, 0, 0, 0, TimeSpan.Zero), open: 2060f, high: 2166f, close: 2153.17f, low: 2053.63f, volume: 14239382),
                new Candle(new DateTimeOffset(2020, 8, 27, 0, 0, 0, 0, TimeSpan.Zero), open: 2180.46f, high: 2295.6f, close: 2238.75f, low: 2142.5f, volume: 23693043),
                new Candle(new DateTimeOffset(2020, 8, 28, 0, 0, 0, 0, TimeSpan.Zero), open: 2295.12f, high: 2318.49f, close: 2213.4f, low: 2186.52f, volume: 20081176),
                new Candle(new DateTimeOffset(2020, 8, 31, 0, 0, 0, 0, TimeSpan.Zero), open: 444.61f, high: 500.14f, close: 498.32f, low: 440.11f, volume: 115847020),
                new Candle(new DateTimeOffset(2020, 9, 1, 0, 0, 0, 0, TimeSpan.Zero), open: 502.14f, high: 502.49f, close: 475.05f, low: 470.51f, volume: 90119419));

            var adjusted = splits.Adjust(candles);
            var expected = SortedCandles.Create(
                new Candle(new DateTimeOffset(2020, 8, 26, 0, 0, 0, 0, TimeSpan.Zero), open: 412f, high: 433.2f, close: 430.634f, low: 410.72598f, volume: 14239382),
                new Candle(new DateTimeOffset(2020, 8, 27, 0, 0, 0, 0, TimeSpan.Zero), open: 436.092f, high: 459.12003f, close: 447.75f, low: 428.5f, volume: 23693043),
                new Candle(new DateTimeOffset(2020, 8, 28, 0, 0, 0, 0, TimeSpan.Zero), open: 459.02402f, high: 463.698f, close: 442.68f, low: 437.30402f, volume: 20081176),
                new Candle(new DateTimeOffset(2020, 8, 31, 0, 0, 0, 0, TimeSpan.Zero), open: 444.61f, high: 500.14f, close: 498.32f, low: 440.11f, volume: 115847020),
                new Candle(new DateTimeOffset(2020, 9, 1, 0, 0, 0, 0, TimeSpan.Zero), open: 502.14f, high: 502.49f, close: 475.05f, low: 470.51f, volume: 90119419));

            CollectionAssert.AreEqual(expected, adjusted);
        }
    }
}
