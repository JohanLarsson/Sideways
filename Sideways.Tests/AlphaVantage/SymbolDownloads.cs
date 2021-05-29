namespace Sideways.Tests.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;
    using NUnit.Framework;
    using Sideways.AlphaVantage;

    public static class SymbolDownloadsTests
    {
        [Test]
        public static void TryCreateWhenUnlisted()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    new AlphaVantageClientSettings
                    {
                        ApiKey = "apiKey",
                        MaxCallsPerMinute = 5,
                    },
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet.Create("I")));
            Assert.AreEqual(null, SymbolDownloads.TryCreate("I", default, default, new Downloader(settings), settings.AlphaVantage));
        }

        [Test]
        public static void TryCreateWhenUpToDate()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    new AlphaVantageClientSettings
                    {
                        ApiKey = "apiKey",
                        MaxCallsPerMinute = 5,
                    },
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet<string>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero);
            Assert.AreEqual(null, SymbolDownloads.TryCreate("TSLA", new TimeRange(end.AddDays(-100), end), new TimeRange(end.AddDays(-100), end), new Downloader(settings), settings.AlphaVantage));
        }

        [Test]
        public static void TryCreateWhenMissingMinutes()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    new AlphaVantageClientSettings
                    {
                        ApiKey = "apiKey",
                        MaxCallsPerMinute = 5,
                    },
                    symbolsWithMissingMinutes: ImmutableSortedSet.Create("ETHE"),
                    unlistedSymbols: ImmutableSortedSet<string>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero);
            Assert.AreEqual(null, SymbolDownloads.TryCreate("ETHE", new TimeRange(end.AddDays(-100), end), default, new Downloader(settings), settings.AlphaVantage));
        }

        [Test]
        public static void TryCreateWhenDaysNeedUpdate()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    new AlphaVantageClientSettings
                    {
                        ApiKey = "apiKey",
                        MaxCallsPerMinute = 5,
                    },
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet<string>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero);
            var symbolDownloads = SymbolDownloads.TryCreate("TSLA", new TimeRange(end.AddDays(-100), end.AddDays(-5)), new TimeRange(end.AddDays(-100), end), new Downloader(settings), settings.AlphaVantage);
            Assert.AreEqual(OutputSize.Compact, symbolDownloads.DaysDownload.OutputSize);
            Assert.AreEqual(0, symbolDownloads.MinutesDownloads.Length);
        }

        [Test]
        public static void TryCreateWhenMinutessNeedUpdate()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    new AlphaVantageClientSettings
                    {
                        ApiKey = "apiKey",
                        MaxCallsPerMinute = 5,
                    },
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet<string>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero);
            var symbolDownloads = SymbolDownloads.TryCreate("TSLA", new TimeRange(end.AddDays(-100), end), new TimeRange(end.AddDays(-100), end.AddDays(-5)), new Downloader(settings), settings.AlphaVantage);
            Assert.AreEqual(null, symbolDownloads.DaysDownload);
            Assert.AreEqual(null, symbolDownloads.MinutesDownloads.Single().Slice);
        }

        [Test]
        public static void TryCreateWhenDaysAndMinutesNeedsUpdate()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    new AlphaVantageClientSettings
                    {
                        ApiKey = "apiKey",
                        MaxCallsPerMinute = 5,
                    },
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet<string>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero).AddDays(-5);
            var symbolDownloads = SymbolDownloads.TryCreate("TSLA", new TimeRange(end.AddDays(-100), end), new TimeRange(end.AddDays(-100), end), new Downloader(settings), settings.AlphaVantage);
            Assert.AreEqual(OutputSize.Compact, symbolDownloads.DaysDownload.OutputSize);
            Assert.AreEqual(null, symbolDownloads.MinutesDownloads.Single().Slice);
        }
    }
}
