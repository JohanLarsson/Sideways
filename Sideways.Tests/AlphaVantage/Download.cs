namespace Sideways.Tests.AlphaVantage
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Sideways.AlphaVantage;

    [Explicit]
    public static class Download
    {
        private static readonly AlphaVantageClientSettings ClientSettings = Settings.FromFile().AlphaVantage.ClientSettings;

        [Test]
        public static async Task Listings()
        {
            using var client = new AlphaVantageClient(new HttpClientHandler(), ClientSettings.ApiKey!, ClientSettings.MaxCallsPerMinute);
            var listings = await client.ListingsAsync().ConfigureAwait(false);
            Database.WriteListings(listings);
        }

        [TestCase("TSLA")]
        public static async Task Earnings(string symbol)
        {
            using var client = new AlphaVantageClient(new HttpClientHandler(), ClientSettings.ApiKey!, ClientSettings.MaxCallsPerMinute);
            var json = await client.GetStringAsync(new Uri($"/query?function=EARNINGS&symbol={symbol}&apikey={ClientSettings.ApiKey!}", UriKind.Relative));
            Console.Write(json);
            var earnings = await client.EarningsAsync(symbol).ConfigureAwait(false);
            Database.WriteAnnualEarnings(earnings.Symbol, earnings.AnnualEarnings);
            Database.WriteQuarterlyEarnings(earnings.Symbol, earnings.QuarterlyEarnings);
        }
    }
}
