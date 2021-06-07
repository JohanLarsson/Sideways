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
    public static class BackTest
    {
        [Test]
        public static void GapUps()
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
                        bookmarks.Add(new Bookmark(symbol, candle.Time, ImmutableSortedSet<string>.Empty, null));
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

            var file = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Bookmarks", "Gap-ups.bookmarks"));
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
                    for (var i = 10; i < candles.Count; i++)
                    {
                        var candle = candles[i];
                        if (Percent(candle.Open, candle.Close) > 0.05 &&
                            candle.Close * candle.Volume > 10_000_000 &&
                            RelativeVolume() > 2)
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

                        static IEnumerable<Candle> Slice(SortedCandles source, int from, int to)
                        {
                            for (var i = from; i <= to; i++)
                            {
                                yield return source[i];
                            }
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
    }
}
