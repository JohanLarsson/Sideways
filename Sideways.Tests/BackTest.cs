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
        private static readonly ImmutableArray<string> Symbols = Database.ReadSymbols();

        [TestCaseSource(nameof(Symbols))]
        public static void GapUp(string symbol)
        {
            var candles = Database.ReadDays(symbol);
            for (var i = 1; i < candles.Count - 5; i++)
            {
                var candle = candles[i];
                if (Gap() > 0.10)
                {
                    Console.WriteLine($"{candle.Time:yyyy-MM-dd} {100 * (candles[i + 5].Close - candle.Open) / candle.Open,6:F1}%");
                }

                double Gap()
                {
                    return (candle.Open - candles[i - 1].High) / candle.Open;
                }
            }
        }

        [Test]
        public static void AllGapUps()
        {
            var bookmarks = new List<Bookmark>();
            foreach (var symbol in Symbols)
            {
                var candles = Database.ReadDays(symbol);
                for (var i = 10; i < candles.Count - 20; i++)
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

            Console.WriteLine(bookmarks.Count);
            File.WriteAllText(
                file.FullName,
                JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true }));
        }
    }
}
