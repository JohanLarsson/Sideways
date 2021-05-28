namespace Sideways.Tests
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Sideways.AlphaVantage;

    [Explicit]
    public static class Download
    {
        private static readonly AlphaVantageClientSettings? ClientSettings = Settings.FromFile()?.AlphaVantage?.ClientSettings;

        [Test]
        public static async Task Listings()
        {
            using var client = new AlphaVantageClient(new HttpClientHandler(), ClientSettings!.ApiKey!, ClientSettings.MaxCallsPerMinute);
            var listings = await client.ListingsAsync().ConfigureAwait(false);
            Database.WriteListings(listings);
        }
    }
}
