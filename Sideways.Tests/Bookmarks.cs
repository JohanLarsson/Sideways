// ReSharper disable UnusedMember.Global
namespace Sideways.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    using NUnit.Framework;

    [Explicit]
    public static class Bookmarks
    {
        [Test]
        public static void EpisodicPivots()
        {
            var bookmarks = new List<Bookmark>();
            foreach (var symbol in Database.ReadSymbols())
            {
                var candles = Database.ReadDays(symbol);
                for (var i = 10; i < candles.Count; i++)
                {
                    var candle = candles[i];
                    if (Percent(candles[i - 1].High, candle.Open) > 0.1 &&
                        candle.Close * candle.Volume > 10_000_000 &&
                        RelativeClose() > 0 &&
                        RelativeVolume() > 0)
                    {
                        bookmarks.Add(new Bookmark(symbol, TradingDay.EndOfDay(candle.Time), ImmutableSortedSet<string>.Empty, null));
                    }

                    double Percent(float start, float end)
                    {
                        return (end - start) / start;
                    }

                    double RelativeVolume()
                    {
                        return candle.Volume / Slice(candles, i - 10, i - 1).Average(x => x.Volume);
                    }

                    double RelativeClose()
                    {
                        return (candle.Close - candle.Low) / (candle.High - candle.Low);
                    }

                    static IEnumerable<Candle> Slice(SortedCandles source, int from, int to)
                    {
                        for (var i = from; i <= to; i++)
                        {
                            yield return source[i];
                        }
                    }
                }
            }

            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Bookmarks", "Episodic pivot.bookmarks"));
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }

            File.WriteAllText(
                file.FullName,
                JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true }));
            Assert.Pass($"Wrote {bookmarks.Count} bookmarks.");
        }

        [Test]
        public static void BigGreen()
        {
            var bookmarks = new List<Bookmark>();
            foreach (var symbol in Database.ReadSymbols())
            {
                if (Database.FirstMinute(symbol) is { } firstMinute)
                {
                    var candles = Database.ReadDays(symbol, firstMinute.Date, DateTimeOffset.Now);
                    foreach (var candle in candles)
                    {
                        if ((candle.Close - candle.Open) / candle.Open > 0.05 &&
                            candle.Close * candle.Volume > 10_000_000)
                        {
                            bookmarks.Add(new Bookmark(symbol, TradingDay.EndOfDay(candle.Time), ImmutableSortedSet<string>.Empty, null));
                        }
                    }
                }
            }

            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Bookmarks", "Big green.bookmarks"));
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }

            File.WriteAllText(
                file.FullName,
                JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true }));
            Assert.Pass($"Wrote {bookmarks.Count} bookmarks.");
        }

        [Test]
        public static void UpThreeDays()
        {
            var bookmarks = new List<Bookmark>();
            foreach (var symbol in Database.ReadSymbols())
            {
                if (Database.FirstMinute(symbol) is { } firstMinute)
                {
                    var candles = Database.ReadDays(symbol, firstMinute.Date, DateTimeOffset.Now);
                    for (var i = 2; i < candles.Count - 2; i++)
                    {
                        var candle = candles[i];
                        if (Change(candle) > 0.02 &&
                            Change(candles[i + 1]) > 0.02 &&
                            Change(candles[i + 2]) > 0.02 &&
                            candle.Close * candle.Volume > 10_000_000)
                        {
                            bookmarks.Add(new Bookmark(symbol, TradingDay.EndOfDay(candle.Time), ImmutableSortedSet<string>.Empty, null));
                        }

                        static double Change(Candle c) => (c.Close - c.Open) / c.Open;
                    }
                }
            }

            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Bookmarks", "Up three days.bookmarks"));
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }

            File.WriteAllText(
                file.FullName,
                JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true }));
            Assert.Pass($"Wrote {bookmarks.Count} bookmarks.");
        }

        [Test]
        public static void StraightUpTwoDays()
        {
            var bookmarks = new List<Bookmark>();
            foreach (var symbol in Database.ReadSymbols())
            {
                if (Database.FirstMinute(symbol) is { } firstMinute)
                {
                    var candles = Database.ReadDays(symbol, firstMinute.Date, DateTimeOffset.Now);
                    for (var i = 2; i < candles.Count - 2; i++)
                    {
                        var candle = candles[i];
                        if (Change(candle) > 0.02 &&
                            Change(candles[i + 1]) > 0.02 &&
                            candles[i + 1].Open >= candle.Close &&
                            candle.Close * candle.Volume > 10_000_000)
                        {
                            bookmarks.Add(new Bookmark(symbol, TradingDay.EndOfDay(candle.Time), ImmutableSortedSet<string>.Empty, null));
                        }

                        static double Change(Candle c) => (c.Close - c.Open) / c.Open;
                    }
                }
            }

            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Bookmarks", "Straight up two days.bookmarks"));
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }

            File.WriteAllText(
                file.FullName,
                JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true }));
            Assert.Pass($"Wrote {bookmarks.Count} bookmarks.");
        }

        [Test]
        public static void UpThenOpenAbove()
        {
            var bookmarks = new List<Bookmark>();
            foreach (var symbol in Database.ReadSymbols())
            {
                if (Database.FirstMinute(symbol) is { } firstMinute)
                {
                    var candles = Database.ReadDays(symbol, firstMinute.Date, DateTimeOffset.Now);
                    for (var i = 2; i < candles.Count - 2; i++)
                    {
                        var candle = candles[i];
                        if (Change(candle) > 0.03 &&
                            RelativeClose() > 0.75 &&
                            candles[i + 1].Open >= candle.Close &&
                            candle.Close * candle.Volume > 10_000_000)
                        {
                            bookmarks.Add(new Bookmark(symbol, TradingDay.EndOfDay(candle.Time), ImmutableSortedSet<string>.Empty, null));
                        }

                        double RelativeClose()
                        {
                            return (candle.Close - candle.Low) / (candle.High - candle.Low);
                        }

                        double Change(Candle c) => (c.Close - c.Open) / c.Open;
                    }
                }
            }

            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Bookmarks", "Up then open above.bookmarks"));
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }

            File.WriteAllText(
                file.FullName,
                JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true }));
            Assert.Pass($"Wrote {bookmarks.Count} bookmarks.");
        }

        [Test]
        public static void EightPercentThreeDays()
        {
            var bookmarks = new List<Bookmark>();
            foreach (var symbol in Database.ReadSymbols())
            {
                var candles = Database.ReadDays(symbol);
                for (var i = 3; i < candles.Count; i++)
                {
                    if (candles[i].Close / candles[i - 3].Open > 1.08 &&
                        candles[i].Close * candles[i].Volume > 10_000_000)
                    {
                        bookmarks.Add(new Bookmark(symbol, TradingDay.StartOfRegularHours(candles[i - 3].Time), ImmutableSortedSet<string>.Empty, null));
                    }
                }
            }

            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Bookmarks", "Eight percent three days.bookmarks"));
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }

            File.WriteAllText(
                file.FullName,
                JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true }));
            Assert.Pass($"Wrote {bookmarks.Count} bookmarks.");
        }

        [Test]
        public static void GapUps()
        {
            var bookmarks = new List<Bookmark>();
            foreach (var symbol in Database.ReadSymbols())
            {
                if (Database.FirstMinute(symbol) is { } firstMinute)
                {
                    var candles = Database.ReadDays(symbol, firstMinute.Date, DateTimeOffset.Now);
                    for (var i = 1; i < candles.Count; i++)
                    {
                        if ((candles[i].Open - candles[i - 1].High) / candles[i - 1].High > 0.01 &&
                            candles[i].Close * candles[i].Volume > 10_000_000)
                        {
                            bookmarks.Add(new Bookmark(symbol, TradingDay.EndOfDay(candles[i].Time), ImmutableSortedSet<string>.Empty, null));
                        }
                    }
                }
            }

            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Bookmarks", "Gap up.bookmarks"));
            if (!Directory.Exists(file.DirectoryName))
            {
                Directory.CreateDirectory(file.DirectoryName!);
            }

            File.WriteAllText(
                file.FullName,
                JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true }));
            Assert.Pass($"Wrote {bookmarks.Count} bookmarks.");
        }
    }
}
