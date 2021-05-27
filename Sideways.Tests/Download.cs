namespace Sideways.Tests
{
    using System.Net.Http;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Sideways.AlphaVantage;

    [Explicit]
    public static class Download
    {
        private static readonly AlphaVantageClient Client = new(new HttpClientHandler(), AlphaVantageClient.ApiKey, 5);

        [Test]
        public static async Task Listings()
        {
            var listings = await Client.ListingsAsync().ConfigureAwait(false);
            Database.WriteListings(listings);
        }
    }
}
