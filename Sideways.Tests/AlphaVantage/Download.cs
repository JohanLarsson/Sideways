﻿namespace Sideways.Tests.AlphaVantage
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
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
            var days = Database.ReadDays(symbol, DateTimeOffset.Now.AddYears(-2), DateTimeOffset.Now.AddDays(-29)).Select(x => x.Time.Date);
            var minuteDays = Database.ReadMinutes(symbol, DateTimeOffset.Now.AddYears(-2), DateTimeOffset.Now.AddDays(-29)).Select(x => x.Time.Date).Distinct();
            if (minuteDays.Any() &&
                !Enumerable.SequenceEqual(days, minuteDays))
            {
                var downloads = new List<Slice>();
                foreach (var slice in Enum.GetValues<Slice>().Where(x => x != Slice.Year1Month1))
                {
                    if (ShouldDownload(slice))
                    {
                        downloads.Add(slice);
                        var minutesDownload = new MinutesDownload(symbol, slice, Downloader);
                        await minutesDownload.ExecuteAsync();
                    }

                    bool ShouldDownload(Slice slice)
                    {
                        var sliceRange = TimeRange.FromSlice(slice);
                        return !Enumerable.SequenceEqual(SliceDays(days), SliceDays(minuteDays));

                        IEnumerable<DateTime> SliceDays(IEnumerable<DateTime> source)
                        {
                            var firstMinute = Settings.AlphaVantage.FirstMinutes.GetValueOrDefault(symbol);
                            return source.Where(x => sliceRange.Contains(x) && x.Date >= firstMinute.Date);
                        }
                    }
                }

                if (downloads.Count == 0)
                {
                    Assert.Pass("No holes!");
                }
                else
                {
                    Assert.Pass(string.Join(", ", downloads));
                }
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
