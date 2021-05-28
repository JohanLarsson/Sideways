﻿namespace Sideways.Tests
{
    using System.Collections.Immutable;
    using System.Text.Json;

    using NUnit.Framework;

    public static class SettingsTests
    {
        [Test]
        public static void JsonRoundtrip()
        {
            var settings = new Settings
            {
                AlphaVantage = new AlphaVantageSettings(
                    new AlphaVantageClientSettings
                    {
                        ApiKey = "apiKey",
                        MaxCallsPerMinute = 5,
                    },
                    symbolsWithMissingMinutes: ImmutableSortedSet.Create("I", "LVGO"),
                    unlistedSymbols: ImmutableSortedSet.Create("I")),
            };

            var json = JsonSerializer.Serialize(settings);
            var read = JsonSerializer.Deserialize<Settings>(json);
            Assert.AreEqual(settings.AlphaVantage.ClientSettings.ApiKey, read!.AlphaVantage!.ClientSettings.ApiKey);
            CollectionAssert.AreEqual(new[] { "I", "LVGO" }, read!.AlphaVantage!.SymbolsWithMissingMinutes);
            CollectionAssert.AreEqual(new[] { "I" }, read!.AlphaVantage!.UnlistedSymbols);
        }
    }
}
