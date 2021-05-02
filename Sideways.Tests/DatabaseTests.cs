namespace Sideways.Tests
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NUnit.Framework;

    public static class DatabaseTests
    {
        private static readonly ImmutableArray<Candle> Candles = ImmutableArray.Create(new Candle(new DateTimeOffset(2021, 04, 15, 00, 00, 00, 0, TimeSpan.Zero), 1.2f, 2.3f, 3.4f, 4.5f, 6));
        //// private static readonly ImmutableArray<Candle> Candles = ReadCandles();

        [Test]
        public static void Write()
        {
            Database.WriteDays("_UNITTEST", Candles);
        }

        [Test]
        public static async Task Read()
        {
            var candles = await Database.ReadDaysAsync("_UNITTEST");
            CollectionAssert.AreEqual(Candles.OrderBy(x => x.Time), candles.OrderBy(x => x.Time));
        }

        [Test]
        public static async Task ReadFromTo()
        {
            var candles = await Database.ReadDaysAsync("_UNITTEST", Candles[0].Time, Candles[0].Time);
            CollectionAssert.AreEqual(Candles.OrderBy(x => x.Time), candles.OrderBy(x => x.Time));
        }

        [Test]
        public static async Task ReadCountTo()
        {
            var candles = await Database.ReadDaysAsync("_UNITTEST", 2, Candles[0].Time);
            CollectionAssert.AreEqual(Candles.OrderBy(x => x.Time), candles.OrderBy(x => x.Time));
        }

#pragma warning disable IDE0051 // Remove unused private members
        private static ImmutableArray<Candle> ReadCandles()
#pragma warning restore IDE0051 // Remove unused private members
        {
            using var stream = File.OpenRead(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"Sideways\\Symbols\\MSFT\\TIME_SERIES_DAILY_ADJUSTED.data"));
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
            return Csv.ParseAdjustedCandlesAsync(stream, Encoding.UTF8).Result.Select(x => new Candle(x.Time, x.Open, x.High, x.Low, x.Close, x.Volume)).ToImmutableArray();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
        }
    }
}
