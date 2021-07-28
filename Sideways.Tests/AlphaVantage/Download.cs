namespace Sideways.Tests.AlphaVantage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Sideways.AlphaVantage;

    [Explicit]
    public static class Download
    {
        private static readonly Settings Settings = Settings.FromFile();
        private static readonly AlphaVantageClientSettings ClientSettings = Settings.AlphaVantage.ClientSettings;
        private static readonly Downloader Downloader = new(Settings);
        private static readonly ImmutableArray<string> Symbols = Database.ReadSymbols();

        [Test]
        public static async Task Listings()
        {
            using var client = new AlphaVantageClient(new HttpClientHandler(), ClientSettings.ApiKey!, ClientSettings.MaxCallsPerMinute);
            var listings = await client.ListingsAsync().ConfigureAwait(false);
            Database.WriteListings(listings);
        }

        [TestCaseSource(nameof(MissingSymbolsSource))]
        public static async Task DaysAndSplitsAsync(string symbol)
        {
            var daysAndSplitsAsync = await Downloader.DaysAndSplitsAsync(symbol);
            CollectionAssert.IsNotEmpty(daysAndSplitsAsync.Candles);
        }

        [TestCaseSource(nameof(Symbols))]
        public static async Task MinuteHoles(string symbol)
        {
            var start = DateTimeOffset.Now.AddYears(-2);
            var end = DateTimeOffset.Now.AddMonths(-1);
            var days = Database.ReadDays(symbol, start, end).Select(x => x.Time.Date).ToArray();
            var minuteDays = Database.ReadMinutes(symbol, start, end).Select(x => x.Time.Date).Distinct().ToArray();
            var firstMinute = Settings.AlphaVantage.FirstMinutes.GetValueOrDefault(symbol);

            if (minuteDays.Any() &&
                !days.SequenceEqual(minuteDays))
            {
                var downloads = new List<Slice>();
                foreach (var slice in Enum.GetValues<Slice>().Where(x => x != Slice.Year1Month1))
                {
                    if (ShouldDownload(slice))
                    {
                        downloads.Add(slice);
                        var minutesDownload = new MinutesDownload(symbol, slice, Downloader);
                        Assert.NotZero(await minutesDownload.ExecuteAsync());
                    }

                    bool ShouldDownload(Slice slice)
                    {
                        var sliceRange = TimeRange.FromSlice(slice);

                        if (days.Count(x => IsInSlice(x)) <= minuteDays.Count(x => IsInSlice(x)))
                        {
                            return false;
                        }

                        var missing = days.Where(x => IsInSlice(x)).Except(minuteDays.Where(x => IsInSlice(x))).ToArray();
                        if (missing.Length > 1)
                        {
                            for (var i = 0; i < missing.Length - 1; i++)
                            {
                                if (Array.IndexOf(days, missing[i]) != Array.IndexOf(days, missing[i + 1]) - 1)
                                {
                                    // Not consecutive
                                    return false;
                                }
                            }
                        }

                        return true;

                        bool IsInSlice(DateTimeOffset date) => sliceRange.Contains(date) && date.Date >= firstMinute.Date;
                    }
                }

                Assert.Pass(downloads.Count == 0 ? "No holes!" : string.Join(", ", downloads));
            }
            else
            {
                Assert.Pass("No holes!");
            }
        }

        [TestCase("TSLA")]
        [TestCase("FSLY")]
        public static async Task Earnings(string symbol)
        {
            using var client = new AlphaVantageClient(new HttpClientHandler(), ClientSettings.ApiKey!, ClientSettings.MaxCallsPerMinute);
            //// var json = await client.GetStringAsync(new Uri($"/query?function=EARNINGS&symbol={symbol}&apikey={ClientSettings.ApiKey!}", UriKind.Relative));
            //// System.Console.Write(json);
            var earnings = await client.EarningsAsync(symbol).ConfigureAwait(false);
            Database.WriteAnnualEarnings(earnings.Symbol, earnings.AnnualEarnings);
            Database.WriteQuarterlyEarnings(earnings.Symbol, earnings.QuarterlyEarnings);
        }

        [TestCaseSource(nameof(Symbols))]
        public static async Task MissingMinutes(string symbol)
        {
            if (Settings.AlphaVantage.SymbolsWithMissingMinutes.Contains(symbol))
            {
                Assert.Pass("No minutes.");
            }

            if (Database.FirstMinute(symbol) is { })
            {
                Assert.Pass("Already downloaded.");
            }

            if (Database.ReadDays(symbol, DateTimeOffset.Now.AddMonths(-8), DateTimeOffset.Now) is { Count: > 20 } days &&
                SymbolDownloads.TryCreate(symbol, Database.DayRange(symbol), default, Downloader, Settings.AlphaVantage) is { } downloads)
            {
                if (days[..20].Adr().Scalar < 5)
                {
                    Assert.Inconclusive("Low ADR.");
                }

                await downloads.DownloadAsync().ConfigureAwait(false);
                Assert.IsNull(downloads.State.Exception);
                Assert.Pass("Downloaded.");
            }
            else
            {
                Assert.Inconclusive("No download.");
            }
        }

        private static IEnumerable<string> MissingSymbolsSource()
        {
            return Database.ReadListings()
                .Select(x => x.Symbol)
                .Where(x => x.Length is > 1 and < 5 && !x.Contains("-"))
                .Except(Symbols)
                .Take(500);
        }
    }
}
