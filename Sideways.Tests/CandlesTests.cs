namespace Sideways.Tests
{
    using System;

    using NUnit.Framework;

    public static class CandlesTests
    {
        [Test]
        public static void Skip()
        {
            var c1 = new Candle(
                new DateTimeOffset(2021, 04, 3, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c2 = new Candle(
                new DateTimeOffset(2021, 04, 2, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);

            var c3 = new Candle(
                new DateTimeOffset(2021, 04, 1, 00, 00, 00, 0, TimeSpan.Zero),
                default,
                default,
                default,
                default,
                default);
            var builder = DescendingCandles.CreateBuilder();
            builder.Add(c1);
            builder.Add(c2);
            builder.Add(c3);
            var candles = new Candles(builder.Create(), default);
            Assert.AreEqual(new DateTimeOffset(2021, 04, 3, 20, 00, 00, 0, TimeSpan.Zero), candles.Skip(c2.Time, CandleInterval.Day, -1));
            Assert.AreEqual(new DateTimeOffset(2021, 04, 1, 20, 00, 00, 0, TimeSpan.Zero), candles.Skip(c2.Time, CandleInterval.Day, 1));
        }
    }
}
