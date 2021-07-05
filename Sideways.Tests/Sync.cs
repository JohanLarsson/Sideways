namespace Sideways.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    using NUnit.Framework;

    [Explicit]
    public static class Sync
    {
        private static readonly FileInfo FlashDrive = new("D:\\Database.sqlite3");

        [Test]
        public static void Rebuild()
        {
            var rebuilt = new FileInfo(Database.DbFile.FullName + ".new");
            Sideways.Sync.CopyAll(Database.DbFile, rebuilt);
        }

        [Test]
        public static void DumpDaysAndEarnings()
        {
            var target = new FileInfo(Path.Combine(Database.DbFile.DirectoryName!, "DaysAndEarnings.sqlite3"));
            Sideways.Sync.CopyDays(Database.DbFile, target);
            Sideways.Sync.CopyDividends(Database.DbFile, target);
            Sideways.Sync.CopySplits(Database.DbFile, target);
            Sideways.Sync.CopyAnnualEarnings(Database.DbFile, target);
            Sideways.Sync.CopyQuarterlyEarnings(Database.DbFile, target);
        }

        [Test]
        public static void DumpMinutes()
        {
            var target = new FileInfo(Path.Combine(Database.DbFile.DirectoryName!, "Minutes.sqlite3"));
            var bookmarks = JsonSerializer.Deserialize<ImmutableList<Bookmark>>(File.ReadAllText(Path.Combine(BookmarksFile.Directory, "May 2021 moves.bookmarks")));
            foreach (var symbol in bookmarks.Select(x => x.Symbol).Distinct())
            {
                Sideways.Sync.CopyMinutes(symbol, Database.DbFile, target);
            }
        }

        [Test]
        public static void CopyListings()
        {
            var rebuilt = new FileInfo(Database.DbFile.FullName + ".new");
            var stopwatch = Stopwatch.StartNew();

            Sideways.Sync.CopyListings(Database.DbFile, rebuilt);
            Console.WriteLine($"Copied listings {stopwatch.Elapsed.TotalSeconds} s.");
        }

        [Test]
        public static void CopyAnnualEarnings()
        {
            var rebuilt = new FileInfo(Database.DbFile.FullName + ".new");
            var stopwatch = Stopwatch.StartNew();

            Sideways.Sync.CopyAnnualEarnings(Database.DbFile, rebuilt);
            Console.WriteLine($"Copied listings {stopwatch.Elapsed.TotalSeconds} s.");
        }

        [Test]
        public static void CopyQuarterlyEarnings()
        {
            var rebuilt = new FileInfo(Database.DbFile.FullName + ".new");
            var stopwatch = Stopwatch.StartNew();

            Sideways.Sync.CopyQuarterlyEarnings(Database.DbFile, rebuilt);
            Console.WriteLine($"Copied listings {stopwatch.Elapsed.TotalSeconds} s.");
        }

        [TestCaseSource(nameof(AppSymbols))]
        public static void OneWayToFlash(string symbol)
        {
            Sideways.Sync.CopyAll(symbol, Database.DbFile, FlashDrive);
        }

        [Test]
        public static void OneWayToFlash()
        {
            Sideways.Sync.CopyAll(Database.DbFile, FlashDrive);
        }

        [TestCaseSource(nameof(FlashSymbols))]
        public static void OneWayToApp(string symbol)
        {
            Sideways.Sync.CopyAll(symbol, FlashDrive, Database.DbFile);
        }

        [Test]
        public static void OneWayToApp()
        {
            Sideways.Sync.CopyAll(FlashDrive, Database.DbFile);
        }

        [TestCaseSource(nameof(Diffs))]
        public static void TwoWay(Diff diff)
        {
            if (diff.AppDays != diff.FlashDays)
            {
                if (diff.AppDays.Contains(diff.FlashDays))
                {
                    CopyDays(diff.Symbol, diff.App, diff.Flash);
                }
                else if (diff.FlashDays.Contains(diff.AppDays))
                {
                    CopyDays(diff.Symbol, diff.Flash, diff.App);
                }
                else
                {
                    CopyDays(diff.Symbol, diff.App, diff.Flash);
                    CopyDays(diff.Symbol, diff.Flash, diff.App);
                }

                static void CopyDays(string symbol, FileInfo source, FileInfo target)
                {
                    Console.WriteLine($"Copy days from {source} to {target}.");
                    Sideways.Sync.CopyDays(symbol, source, target);
                    Sideways.Sync.CopySplits(symbol, source, target);
                    Sideways.Sync.CopyDividends(symbol, source, target);
                }
            }

            if (diff.AppMinutes != diff.FlashMinutes)
            {
                if (diff.AppMinutes.Contains(diff.FlashMinutes))
                {
                    Sideways.Sync.CopyMinutes(diff.Symbol, diff.App, diff.Flash);
                }
                else if (diff.FlashMinutes.Contains(diff.AppMinutes))
                {
                    Sideways.Sync.CopyMinutes(diff.Symbol, diff.Flash, diff.App);
                }
                else
                {
                    Sideways.Sync.CopyMinutes(diff.Symbol, diff.App, diff.Flash);
                    Sideways.Sync.CopyMinutes(diff.Symbol, diff.Flash, diff.App);
                }
            }
        }

        private static IEnumerable<string> AppSymbols() => File.Exists(FlashDrive.FullName) ? Database.ReadSymbols() : Enumerable.Empty<string>();

        private static IEnumerable<string> FlashSymbols() => File.Exists(FlashDrive.FullName) ? Database.ReadSymbols(FlashDrive) : Enumerable.Empty<string>();

        private static IEnumerable<Diff> Diffs()
        {
            if (File.Exists(FlashDrive.FullName))
            {
                var dbDays = Database.DayRanges(AppSymbols(), Database.DbFile);
                var dbMinutes = Database.MinuteRanges(AppSymbols(), Database.DbFile);

                var flashDays = Database.DayRanges(FlashSymbols(), FlashDrive);
                var flashMinutes = Database.MinuteRanges(FlashSymbols(), FlashDrive);

                foreach (var symbol in dbDays.Keys.Concat(flashDays.Keys).Distinct().OrderBy(x => x))
                {
                    yield return new Diff(
                        Database.DbFile,
                        FlashDrive,
                        symbol,
                        GetRangeOrDefault(dbDays),
                        GetRangeOrDefault(dbMinutes),
                        GetRangeOrDefault(flashDays),
                        GetRangeOrDefault(flashMinutes));

                    TimeRange GetRangeOrDefault(ImmutableDictionary<string, TimeRange> map)
                    {
                        return map.TryGetValue(symbol, out var range) ? range : default;
                    }
                }
            }
        }

        public sealed class Diff
        {
            public readonly FileInfo App;
            public readonly FileInfo Flash;
            public readonly string Symbol;
            public readonly TimeRange AppDays;
            public readonly TimeRange AppMinutes;
            public readonly TimeRange FlashDays;
            public readonly TimeRange FlashMinutes;

            public Diff(FileInfo app, FileInfo flash, string symbol, TimeRange appDays, TimeRange appMinutes, TimeRange flashDays, TimeRange flashMinutes)
            {
                this.App = app;
                this.Flash = flash;
                this.Symbol = symbol;
                this.AppDays = appDays;
                this.AppMinutes = appMinutes;
                this.FlashDays = flashDays;
                this.FlashMinutes = flashMinutes;
            }

            public override string ToString() => this.Symbol;
        }
    }
}
