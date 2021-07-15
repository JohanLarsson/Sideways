﻿namespace Sideways.Tests.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

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
        public static void SymbolsWithMissingMinutes()
        {
            var settings = new AlphaVantageSettings(
                AlphaVantageClientSettings,
                symbolsWithMissingMinutes: ImmutableSortedSet.Create("ACHN"),
                unlistedSymbols: ImmutableSortedSet<string>.Empty,
                firstMinutes: ImmutableSortedDictionary<string, DateTimeOffset>.Empty);
            CollectionAssert.IsEmpty(MinutesDownload.Create("ACHN", default, default, null!, settings));
        }

        [Test]
        public static void UnlistedComplete()
        {
            var settings = new AlphaVantageSettings(
                AlphaVantageClientSettings,
                symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                unlistedSymbols: ImmutableSortedSet.Create("I"),
                firstMinutes: ImmutableSortedDictionary<string, DateTimeOffset>.Empty);
            var existingDays = new TimeRange(new DateTimeOffset(2013, 4, 18, 0, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2020, 5, 18, 0, 0, 0, 0, TimeSpan.Zero));
            var existingMinutes = new TimeRange(new DateTimeOffset(2018, 4, 18, 0, 0, 0, 0, TimeSpan.Zero), new DateTimeOffset(2020, 5, 18, 0, 0, 0, 0, TimeSpan.Zero));
            CollectionAssert.IsEmpty(MinutesDownload.Create("I", existingDays, existingMinutes, null!, settings));
        }

        [Test]
        public static void WhenNoMinutesDownloaded()
        {
            var settings = new Settings(new AlphaVantageSettings(
                AlphaVantageClientSettings,
                symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                unlistedSymbols: ImmutableSortedSet<string>.Empty,
                firstMinutes: ImmutableSortedDictionary<string, DateTimeOffset>.Empty));
            var existingDays = new TimeRange(new DateTimeOffset(2013, 4, 18, 0, 0, 0, 0, TimeSpan.Zero), DateTimeOffset.Now);
            CollectionAssert.AreEqual(Enum.GetValues<Slice>(), MinutesDownload.Create("TSLA", existingDays, default, new Downloader(settings), settings.AlphaVantage).Select(x => x.Slice));
        }

        [Test]
        public static void LastMonthOnly()
        {
            var settings = new Settings(new AlphaVantageSettings(
                AlphaVantageClientSettings,
                symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                unlistedSymbols: ImmutableSortedSet<string>.Empty,
                firstMinutes: ImmutableSortedDictionary<string, DateTimeOffset>.Empty));
            var existingDays = new TimeRange(new DateTimeOffset(2013, 4, 18, 0, 0, 0, 0, TimeSpan.Zero), DateTimeOffset.Now);
            var existingMinutes = new TimeRange(new DateTimeOffset(2018, 4, 18, 0, 0, 0, 0, TimeSpan.Zero), DateTimeOffset.Now.AddDays(-25));
            CollectionAssert.AreEqual(new Slice?[] { null }, MinutesDownload.Create("TSLA", existingDays, existingMinutes, new Downloader(settings), settings.AlphaVantage).Select(x => x.Slice));
        }
    }
}