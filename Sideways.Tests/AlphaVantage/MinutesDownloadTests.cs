namespace Sideways.Tests.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using NUnit.Framework;

    using Sideways.AlphaVantage;

    public static class MinutesDownloadTests
    {
        private static readonly AlphaVantageClientSettings AlphaVantageClientSettings = new()
        {
            ApiKey = "apiKey",
            MaxCallsPerMinute = 5,
        };

        [Test]
        public static void WhenSymbolsWithMissingMinutes()
        {
            var settings = new AlphaVantageSettings(
                AlphaVantageClientSettings,
                symbolsWithMissingMinutes: ImmutableSortedSet.Create("ACHN"),
                unlistedSymbols: ImmutableSortedSet<string>.Empty,
                firstMinutes: ImmutableSortedDictionary<string, DateTimeOffset>.Empty);
            CollectionAssert.IsEmpty(MinutesDownload.Create("ACHN", default, default, null, settings));
        }
    }
}
