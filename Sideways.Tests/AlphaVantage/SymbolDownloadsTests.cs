namespace Sideways.Tests.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.Linq;

    using NUnit.Framework;

    using Sideways.AlphaVantage;

    public static class SymbolDownloadsTests
    {
        private static readonly AlphaVantageClientSettings AlphaVantageClientSettings = new()
        {
            ApiKey = "apiKey",
            MaxCallsPerMinute = 5,
        };

        [Test]
        public static void TryCreateWhenUnlisted()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    AlphaVantageClientSettings,
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet.Create("I"),
                    firstDayWithMinutes: ImmutableDictionary<string, TradingDay>.Empty));
            Assert.AreEqual(null, SymbolDownloads.TryCreate("I", default, default, new Downloader(settings), settings.AlphaVantage));
        }

        [Test]
        public static void TryCreateWhenUpToDate()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    AlphaVantageClientSettings,
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet<string>.Empty,
                    firstDayWithMinutes: ImmutableDictionary<string, TradingDay>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero);
            Assert.AreEqual(null, SymbolDownloads.TryCreate("TSLA", new TimeRange(end.AddDays(-100), end), new TimeRange(end.AddDays(-100), end), new Downloader(settings), settings.AlphaVantage));
        }

        [Test]
        public static void TryCreateWhenMissingMinutes()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    AlphaVantageClientSettings,
                    symbolsWithMissingMinutes: ImmutableSortedSet.Create("ETHE"),
                    unlistedSymbols: ImmutableSortedSet<string>.Empty,
                    firstDayWithMinutes: ImmutableDictionary<string, TradingDay>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero);
            Assert.AreEqual(null, SymbolDownloads.TryCreate("ETHE", new TimeRange(end.AddDays(-100), end), default, new Downloader(settings), settings.AlphaVantage));
        }

        [Test]
        public static void TryCreateWhenDaysNeedUpdate()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    AlphaVantageClientSettings,
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet<string>.Empty,
                    firstDayWithMinutes: ImmutableDictionary<string, TradingDay>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero);
            var symbolDownloads = SymbolDownloads.TryCreate("TSLA", new TimeRange(end.AddDays(-100), end.AddDays(-5)), new TimeRange(end.AddDays(-100), end), new Downloader(settings), settings.AlphaVantage);
            Assert.AreEqual(OutputSize.Compact, symbolDownloads.DaysDownload.OutputSize);
            Assert.AreEqual(0, symbolDownloads.MinutesDownloads.Length);
        }

        [Test]
        public static void TryCreateWhenMinutesNeedUpdate()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    AlphaVantageClientSettings,
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet<string>.Empty,
                    firstDayWithMinutes: ImmutableDictionary<string, TradingDay>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero);
            var symbolDownloads = SymbolDownloads.TryCreate("TSLA", new TimeRange(end.AddDays(-100), end), new TimeRange(end.AddDays(-100), end.AddDays(-5)), new Downloader(settings), settings.AlphaVantage);
            Assert.AreEqual(null, symbolDownloads.DaysDownload);
            Assert.AreEqual(null, symbolDownloads.MinutesDownloads.Single().Slice);
        }

        [Test]
        public static void TryCreateWhenEmptyMinutes()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    AlphaVantageClientSettings,
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet<string>.Empty,
                    firstDayWithMinutes: ImmutableDictionary<string, TradingDay>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero);
            var symbolDownloads = SymbolDownloads.TryCreate("TSLA", new TimeRange(end.AddYears(-3), end), default, new Downloader(settings), settings.AlphaVantage);
            Assert.AreEqual(null, symbolDownloads.DaysDownload);
            Assert.AreEqual(Enum.GetValues<Slice>(), symbolDownloads.MinutesDownloads.Select(x => x.Slice));
        }

        [Test]
        public static void TryCreateWhenMinutesFillDownAll()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    AlphaVantageClientSettings,
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet<string>.Empty,
                    firstDayWithMinutes: ImmutableDictionary<string, TradingDay>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero);
            var symbolDownloads = SymbolDownloads.TryCreate("TSLA", new TimeRange(end.AddYears(-3), end), new TimeRange(end.AddDays(-40), end), new Downloader(settings), settings.AlphaVantage);
            Assert.AreEqual(null, symbolDownloads.DaysDownload);
            Assert.AreEqual(Enum.GetValues<Slice>().Except(new[] { Slice.Year1Month1 }), symbolDownloads.MinutesDownloads.Select(x => x.Slice));
        }

        [Test]
        public static void TryCreateWhenDaysAndMinutesNeedsUpdate()
        {
            var settings = new Settings(
                alphaVantage: new AlphaVantageSettings(
                    AlphaVantageClientSettings,
                    symbolsWithMissingMinutes: ImmutableSortedSet<string>.Empty,
                    unlistedSymbols: ImmutableSortedSet<string>.Empty,
                    firstDayWithMinutes: ImmutableDictionary<string, TradingDay>.Empty));
            var last = TradingDay.LastComplete();
            var end = new DateTimeOffset(last.Year, last.Month, last.Day, 0, 0, 0, TimeSpan.Zero).AddDays(-5);
            var symbolDownloads = SymbolDownloads.TryCreate("TSLA", new TimeRange(end.AddDays(-100), end), new TimeRange(end.AddDays(-100), end), new Downloader(settings), settings.AlphaVantage);
            Assert.AreEqual(OutputSize.Compact, symbolDownloads.DaysDownload.OutputSize);
            Assert.AreEqual(null, symbolDownloads.MinutesDownloads.Single().Slice);
        }
    }
}
