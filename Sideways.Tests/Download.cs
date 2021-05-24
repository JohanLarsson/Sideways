namespace Sideways.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Sideways.AlphaVantage;

    [Explicit]
    public static class Download
    {
        private static readonly Downloader Downloader = new();
        private static readonly AlphaVantageClient Client = new(new HttpClientHandler(), ApiKey);

        private static string ApiKey
        {
            get
            {
                var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways/AlphaVantage.key");
                if (File.Exists(fileName))
                {
                    return File.ReadAllText(fileName).Trim();
                }

                throw new InvalidOperationException($"Expected the API key in {fileName}");
            }
        }

        [Test]
        public static async Task Listings()
        {
            using var client = new AlphaVantageClient(new HttpClientHandler(), ApiKey);
            var listings = await client.ListingsAsync().ConfigureAwait(false);
            Database.WriteListings(listings);
        }

        [TestCaseSource(nameof(All))]
        public static async Task Days(string symbol)
        {
            var dataSource = new DataSource(Downloader);
            if (dataSource.Days(symbol, Client).Download is { } task)
            {
                await task;
            }
            else
            {
                Assert.Pass("Already downloaded.");
            }
        }

        [TestCaseSource(nameof(EmptyMinutes))]
        public static async Task Minutes(string symbol, Slice slice)
        {
            if (slice != Slice.Year1Month1 &&
                TimeRange.FromSlice(slice) is var range &&
                Database.ReadMinutes(symbol, range.Min.AddDays(1), range.Max.AddDays(5)).Count == 0)
            {
                Assert.Pass("No slice this far back.");
            }

            using var client = new AlphaVantageClient(new HttpClientHandler(), ApiKey);
            var candles = await client.IntradayExtendedAsync(symbol, Interval.Minute, slice, adjusted: false);
            if (candles.IsEmpty)
            {
                Assert.Inconclusive("Empty slice, maybe missing data on AlphaVantage. Exclude this symbol from script as it uses up daily calls.");
            }

            Database.WriteMinutes(symbol, candles);
        }

        [TestCaseSource(nameof(TopUps))]
        public static async Task TopUp(string symbol)
        {
            using var client = new AlphaVantageClient(new HttpClientHandler(), ApiKey);
            var candles = await client.IntradayAsync(symbol, Interval.Minute, adjusted: false);
            if (candles.IsEmpty)
            {
                Assert.Inconclusive("Empty slice, maybe missing data on AlphaVantage. Exclude this symbol from script as it uses up daily calls.");
            }

            Database.WriteMinutes(symbol, candles);
            await Days(symbol);
            ////Assert.Fail("Check that we merge correctly first.");
            ////Assert.Fail("Adjust merged day to midnight.");
            ////var days = candles.Where(x => TradingDay.IsOrdinaryHours(x.Time)).MergeBy((x, y) => x.Time.IsSameDay(y.Time)).Select(x => x.WithTime(new DateTimeOffset(x.Time.Year, x.Time.Month, x.Time.Day, 0, 0, 0, x.Time.Offset)));
            ////Database.WriteDays(symbol, days);
        }

        private static IEnumerable<string> All() => Database.ReadSymbols();

        private static IEnumerable<TestCaseData> EmptyMinutes()
        {
            var ignore = new[]
            {
                "ACHN",
                "AMTD",
                "APPM",
                "AVX",
                "AVXS",
                "BIGG",
                "CHFS",
                "CTRP",
                "CXO",
                "DERM",
                "DGAZ",
                "DNR",
                "DO",
                "DRYS",
                "DWT",
                "EIDX",
                "ERI",
                "ETCG",
                "ETHE",
                "EWLU",
                "FBM",
                "FIT",
                "FTSV",
                "GBTC",
                "GSX",
                "HTZ",
                "I",
                "IDGX",
                "IDXG",
                "IMMU",
                "LK",
                "MNK",
                "NLNK",
                "OVAS",
                "PASO",
                "PNAT",
                "PTI",
            };

            foreach (var (symbol, range) in Database.DayRanges().OrderBy(x => x.Key))
            {
                if (!ignore.Contains(symbol) &&
                    Database.CountMinutes(symbol, Database.DbFile) == 0)
                {
                    foreach (var slice in Enum.GetValues<Slice>())
                    {
                        if (range.Overlaps(TimeRange.FromSlice(slice)))
                        {
                            yield return new TestCaseData(symbol, slice);
                        }
                    }
                }
            }
        }

        private static IEnumerable<string> TopUps()
        {
            foreach (var (symbol, range) in Database.DayRanges().OrderBy(x => x.Key))
            {
                if (TradingDay.Create(range.Max) != TradingDay.LastComplete() &&
                    range.Overlaps(TimeRange.FromSlice(Slice.Year1Month1)))
                {
                    yield return symbol;
                }
            }
        }
    }
}
