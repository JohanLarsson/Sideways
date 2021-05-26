﻿namespace Sideways.Tests
{
    using System;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    using Sideways.AlphaVantage;

    public static class DatabaseTests
    {
        private static readonly ImmutableArray<AdjustedCandle> DayCandles = ImmutableArray.Create(new AdjustedCandle(new DateTimeOffset(2021, 04, 15, 00, 00, 00, 0, TimeSpan.Zero), 1.2f, 2.3f, 3.4f, 4.5f, 5.6f, 7, 8.9f, 9.1f));
        private static readonly ImmutableArray<Candle> MinuteCandles = ImmutableArray.Create(new Candle(new DateTimeOffset(2021, 04, 15, 00, 00, 00, 0, TimeSpan.Zero), 1.2f, 2.3f, 3.4f, 4.5f, 6));
        private static readonly FileInfo DbFile = new(Path.Combine(Path.GetTempPath(), "Sideways", "Database.sqlite3"));

        [Test]
        public static void Days()
        {
            Database.WriteDays("UNIT_TEST", DayCandles, DbFile);

            var candles = Database.ReadDays("UNIT_TEST", DbFile);
            CollectionAssert.AreEqual(DayCandles.Select(x => x.AsCandle(1)).OrderBy(x => x.Time), candles.OrderBy(x => x.Time));

            candles = Database.ReadDays("UNIT_TEST", DayCandles.Min(x => x.Time), DayCandles.Max(x => x.Time), DbFile);
            CollectionAssert.AreEqual(DayCandles.Select(x => x.AsCandle(1)).OrderBy(x => x.Time), candles.OrderBy(x => x.Time));

            Database.WriteDays("UNIT_TEST", DayCandles.Select(x => x.AsCandle(2)), DbFile);
            candles = Database.ReadDays("UNIT_TEST", DbFile);
            CollectionAssert.AreEqual(DayCandles.Select(x => x.AsCandle(1)).OrderBy(x => x.Time), candles.OrderBy(x => x.Time));
        }

        [Test]
        public static void WriteMinutes()
        {
            Database.WriteMinutes("UNIT_TEST", MinuteCandles, DbFile);

            var candles = Database.ReadMinutes("UNIT_TEST", DbFile);
            CollectionAssert.AreEqual(MinuteCandles.OrderBy(x => x.Time), candles.OrderBy(x => x.Time));

            candles = Database.ReadMinutes("UNIT_TEST", DayCandles.Min(x => x.Time), DayCandles.Max(x => x.Time), DbFile);
            CollectionAssert.AreEqual(MinuteCandles.OrderBy(x => x.Time), candles.OrderBy(x => x.Time));
        }

        [Explicit]
        [Test]
        public static void Timings()
        {
            var stopwatch = Stopwatch.StartNew();
            var days = Database.ReadDays("TSLA");
            stopwatch.Stop();
            Console.WriteLine($"Read {days.Count} days took {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Restart();
            var minutes = Database.ReadMinutes("TSLA");
            stopwatch.Stop();
            Console.WriteLine($"Read {minutes.Count} minutes took {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Restart();
            minutes = Database.ReadMinutes("TSLA", new DateTimeOffset(2021, 01, 01, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2021, 02, 01, 0, 0, 0, TimeSpan.Zero));
            stopwatch.Stop();
            Console.WriteLine($"Read {minutes.Count} minutes took {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Restart();
            minutes = Database.ReadMinutes("TSLA", new DateTimeOffset(2021, 01, 01, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2021, 01, 07, 0, 0, 0, TimeSpan.Zero));
            stopwatch.Stop();
            Console.WriteLine($"Read {minutes.Count} minutes took {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Restart();
            var n = Database.CountMinutes("TSLA");
            stopwatch.Stop();
            Console.WriteLine($"Count {n} minutes took {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Restart();
            var symbols = Database.ReadSymbols();
            stopwatch.Stop();
            Console.WriteLine($"Read {symbols.Length} symbols took {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Restart();
            var dayRanges = Database.DayRanges();
            stopwatch.Stop();
            Console.WriteLine($"Read {dayRanges.Count} day ranges took {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Restart();
            var minuteRanges = Database.MinuteRanges();
            stopwatch.Stop();
            Console.WriteLine($"Read {minuteRanges.Count} minute ranges took {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
