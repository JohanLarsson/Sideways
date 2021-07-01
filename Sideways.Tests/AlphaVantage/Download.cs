namespace Sideways.Tests.AlphaVantage
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Sideways.AlphaVantage;

    [Explicit]
    public static class Download
    {
        private static readonly AlphaVantageClientSettings ClientSettings = Settings.FromFile().AlphaVantage.ClientSettings;
        private static readonly Downloader Downloader = new(Settings.FromFile());

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
            await Downloader.DaysAndSplitsAsync(symbol);
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
               .Except(Database.ReadSymbols());
        }
    }
}
