namespace Sideways.Tests.Internal
{
    using System;
    using System.Globalization;
    using System.Linq;
    using NUnit.Framework;

    using Sideways;

    public static class SpanExtensionsTests
    {
        [Test]
        public static void Adr()
        {
            var candles = new[]
            {
                /* 0*/ new Candle(time: new DateTimeOffset(2021, 5, 4, 0, 0, 0, 0, TimeSpan.Zero), open: 22.51f, high: 25.75f, low: 21.86f, close: 24.13f, volume: 3029191),
                /* 1*/ new Candle(time: new DateTimeOffset(2021, 5, 5, 0, 0, 0, 0, TimeSpan.Zero), open: 24.04f, high: 24.41f, low: 22.45f, close: 22.8f, volume: 1328572),
                /* 2*/ new Candle(time: new DateTimeOffset(2021, 5, 6, 0, 0, 0, 0, TimeSpan.Zero), open: 22.5f, high: 23.98f, low: 22.11f, close: 23.4f, volume: 1934735),
                /* 3*/ new Candle(time: new DateTimeOffset(2021, 5, 7, 0, 0, 0, 0, TimeSpan.Zero), open: 23.25f, high: 23.97f, low: 22.79f, close: 23.45f, volume: 1342472),
                /* 4*/ new Candle(time: new DateTimeOffset(2021, 5, 10, 0, 0, 0, 0, TimeSpan.Zero), open: 23.17f, high: 23.17f, low: 21.8f, close: 22.23f, volume: 1167065),
                /* 5*/ new Candle(time: new DateTimeOffset(2021, 5, 11, 0, 0, 0, 0, TimeSpan.Zero), open: 20.24f, high: 22.86f, low: 20.01f, close: 22.59f, volume: 1549932),
                /* 6*/ new Candle(time: new DateTimeOffset(2021, 5, 12, 0, 0, 0, 0, TimeSpan.Zero), open: 22.6f, high: 23.34f, low: 21.82f, close: 22.52f, volume: 1184779),
                /* 7*/ new Candle(time: new DateTimeOffset(2021, 5, 13, 0, 0, 0, 0, TimeSpan.Zero), open: 22.18f, high: 23.21f, low: 20.72f, close: 21.4f, volume: 1961903),
                /* 8*/ new Candle(time: new DateTimeOffset(2021, 5, 14, 0, 0, 0, 0, TimeSpan.Zero), open: 21.99f, high: 22.86f, low: 20.62f, close: 21.09f, volume: 2124684),
                /* 9*/ new Candle(time: new DateTimeOffset(2021, 5, 17, 0, 0, 0, 0, TimeSpan.Zero), open: 20.23f, high: 21.49f, low: 19.75f, close: 21.37f, volume: 1650646),
                /*10*/ new Candle(time: new DateTimeOffset(2021, 5, 18, 0, 0, 0, 0, TimeSpan.Zero), open: 20.92f, high: 25.18f, low: 20.45f, close: 23.45f, volume: 4006906),
                /*11*/ new Candle(time: new DateTimeOffset(2021, 5, 19, 0, 0, 0, 0, TimeSpan.Zero), open: 22.14f, high: 22.65f, low: 21.1f, close: 21.96f, volume: 2576531),
                /*12*/ new Candle(time: new DateTimeOffset(2021, 5, 20, 0, 0, 0, 0, TimeSpan.Zero), open: 22.3f, high: 22.99f, low: 21.51f, close: 22.81f, volume: 1320736),
                /*13*/ new Candle(time: new DateTimeOffset(2021, 5, 21, 0, 0, 0, 0, TimeSpan.Zero), open: 23.2f, high: 23.81f, low: 21.61f, close: 21.74f, volume: 1653603),
                /*14*/ new Candle(time: new DateTimeOffset(2021, 5, 24, 0, 0, 0, 0, TimeSpan.Zero), open: 22f, high: 22.17f, low: 20.85f, close: 21.6f, volume: 1550110),
                /*15*/ new Candle(time: new DateTimeOffset(2021, 5, 25, 0, 0, 0, 0, TimeSpan.Zero), open: 21.6f, high: 24.89f, low: 21.17f, close: 22.53f, volume: 6794783),
                /*16*/ new Candle(time: new DateTimeOffset(2021, 5, 26, 0, 0, 0, 0, TimeSpan.Zero), open: 23f, high: 26.39f, low: 22.3f, close: 25.47f, volume: 6576354),
                /*17*/ new Candle(time: new DateTimeOffset(2021, 5, 27, 0, 0, 0, 0, TimeSpan.Zero), open: 25.85f, high: 26.4f, low: 24.33f, close: 25.86f, volume: 3550413),
                /*18*/ new Candle(time: new DateTimeOffset(2021, 5, 28, 0, 0, 0, 0, TimeSpan.Zero), open: 26.41f, high: 28.38f, low: 25.8f, close: 27.01f, volume: 2495548),
                /*19*/ new Candle(time: new DateTimeOffset(2021, 6, 1, 0, 0, 0, 0, TimeSpan.Zero), open: 27.95f, high: 29.85f, low: 27.5f, close: 28.54f, volume: 2857094),
            };
            Assert.AreEqual("10.8%", SpanExtensions.Adr(candles.AsSpan()).ToString("#.#", CultureInfo.InvariantCulture));
        }

        [Test]
        public static void Atr()
        {
            var candles = new[]
            {
                /* 0*/ new Candle(time: new DateTimeOffset(2021, 5, 3, 0, 0, 0, 0, TimeSpan.Zero), open: 27.01f, high: 27.27f, low: 22.8f, close: 23.19f, volume: 5263216),
                /* 1*/ new Candle(time: new DateTimeOffset(2021, 5, 4, 0, 0, 0, 0, TimeSpan.Zero), open: 22.51f, high: 25.75f, low: 21.86f, close: 24.13f, volume: 3029191),
                /* 2*/ new Candle(time: new DateTimeOffset(2021, 5, 5, 0, 0, 0, 0, TimeSpan.Zero), open: 24.04f, high: 24.41f, low: 22.45f, close: 22.8f, volume: 1328572),
                /* 3*/ new Candle(time: new DateTimeOffset(2021, 5, 6, 0, 0, 0, 0, TimeSpan.Zero), open: 22.5f, high: 23.98f, low: 22.11f, close: 23.4f, volume: 1934735),
                /* 4*/ new Candle(time: new DateTimeOffset(2021, 5, 7, 0, 0, 0, 0, TimeSpan.Zero), open: 23.25f, high: 23.97f, low: 22.79f, close: 23.45f, volume: 1342472),
                /* 5*/ new Candle(time: new DateTimeOffset(2021, 5, 10, 0, 0, 0, 0, TimeSpan.Zero), open: 23.17f, high: 23.17f, low: 21.8f, close: 22.23f, volume: 1167065),
                /* 6*/ new Candle(time: new DateTimeOffset(2021, 5, 11, 0, 0, 0, 0, TimeSpan.Zero), open: 20.24f, high: 22.86f, low: 20.01f, close: 22.59f, volume: 1549932),
                /* 7*/ new Candle(time: new DateTimeOffset(2021, 5, 12, 0, 0, 0, 0, TimeSpan.Zero), open: 22.6f, high: 23.34f, low: 21.82f, close: 22.52f, volume: 1184779),
                /* 8*/ new Candle(time: new DateTimeOffset(2021, 5, 13, 0, 0, 0, 0, TimeSpan.Zero), open: 22.18f, high: 23.21f, low: 20.72f, close: 21.4f, volume: 1961903),
                /* 9*/ new Candle(time: new DateTimeOffset(2021, 5, 14, 0, 0, 0, 0, TimeSpan.Zero), open: 21.99f, high: 22.86f, low: 20.62f, close: 21.09f, volume: 2124684),
                /*10*/ new Candle(time: new DateTimeOffset(2021, 5, 17, 0, 0, 0, 0, TimeSpan.Zero), open: 20.23f, high: 21.49f, low: 19.75f, close: 21.37f, volume: 1650646),
                /*11*/ new Candle(time: new DateTimeOffset(2021, 5, 18, 0, 0, 0, 0, TimeSpan.Zero), open: 20.92f, high: 25.18f, low: 20.45f, close: 23.45f, volume: 4006906),
                /*12*/ new Candle(time: new DateTimeOffset(2021, 5, 19, 0, 0, 0, 0, TimeSpan.Zero), open: 22.14f, high: 22.65f, low: 21.1f, close: 21.96f, volume: 2576531),
                /*13*/ new Candle(time: new DateTimeOffset(2021, 5, 20, 0, 0, 0, 0, TimeSpan.Zero), open: 22.3f, high: 22.99f, low: 21.51f, close: 22.81f, volume: 1320736),
                /*14*/ new Candle(time: new DateTimeOffset(2021, 5, 21, 0, 0, 0, 0, TimeSpan.Zero), open: 23.2f, high: 23.81f, low: 21.61f, close: 21.74f, volume: 1653603),
                /*15*/ new Candle(time: new DateTimeOffset(2021, 5, 24, 0, 0, 0, 0, TimeSpan.Zero), open: 22f, high: 22.17f, low: 20.85f, close: 21.6f, volume: 1550110),
                /*16*/ new Candle(time: new DateTimeOffset(2021, 5, 25, 0, 0, 0, 0, TimeSpan.Zero), open: 21.6f, high: 24.89f, low: 21.17f, close: 22.53f, volume: 6794783),
                /*17*/ new Candle(time: new DateTimeOffset(2021, 5, 26, 0, 0, 0, 0, TimeSpan.Zero), open: 23f, high: 26.39f, low: 22.3f, close: 25.47f, volume: 6576354),
                /*18*/ new Candle(time: new DateTimeOffset(2021, 5, 27, 0, 0, 0, 0, TimeSpan.Zero), open: 25.85f, high: 26.4f, low: 24.33f, close: 25.86f, volume: 3550413),
                /*19*/ new Candle(time: new DateTimeOffset(2021, 5, 28, 0, 0, 0, 0, TimeSpan.Zero), open: 26.41f, high: 28.38f, low: 25.8f, close: 27.01f, volume: 2495548),
                /*20*/ new Candle(time: new DateTimeOffset(2021, 6, 1, 0, 0, 0, 0, TimeSpan.Zero), open: 27.95f, high: 29.85f, low: 27.5f, close: 28.54f, volume: 2857094),
            };
            Assert.AreEqual("2.4", SpanExtensions.Atr(candles.AsSpan()).ToString("#.#", CultureInfo.InvariantCulture));
            Assert.AreEqual("2.4", SpanExtensions.Atr(candles.Reverse().ToArray().AsSpan()).ToString("#.#", CultureInfo.InvariantCulture));
        }
    }
}
