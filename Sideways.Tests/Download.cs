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

        [TestCaseSource(nameof(MissingMinutes))]
        public static async Task Minutes(string symbol, Slice slice)
        {
            using var client = new AlphaVantageClient(new HttpClientHandler(), ApiKey);
            var candles = await client.IntervalExtendedAsync(symbol, Interval.Minute, slice, adjusted: false);
            if (candles.IsEmpty)
            {
                Assert.Inconclusive("Empty slice, maybe missing data on AlphaVantage. Exclude this symbol from script as it uses up daily calls.");
            }

            Database.WriteMinutes(symbol, candles);
        }

        private static IEnumerable<string> All() => Database.ReadSymbols();

        private static IEnumerable<TestCaseData> MissingMinutes()
        {
            return Core().Take(500);

            static IEnumerable<TestCaseData> Core()
            {
                foreach (var symbol in Database.ReadSymbols())
                {
                    if (Sideways.Sync.CountMinutes(symbol, Database.DbFile) == 0)
                    {
                        var days = Database.ReadDays(symbol);
                        if (days.Count > 0)
                        {
                            foreach (var slice in Enum.GetValues<Slice>())
                            {
                                if (days.IndexOf(TimeRange.FromSlice(slice).Max, 0) >= 0)
                                {
                                    yield return new TestCaseData(symbol, slice);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
