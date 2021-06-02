namespace Sideways.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    [Explicit]
    public static class Sync
    {
        private static readonly FileInfo FlashDrive = new("D:\\Database.sqlite3");

        [Test]
        public static void Rebuild()
        {
            var rebuilt = new FileInfo(Database.DbFile.FullName + ".new");
            var stopwatch = Stopwatch.StartNew();
            _ = Database.ReadSymbols(rebuilt);
            Sideways.Sync.CopyDays(Database.DbFile, rebuilt);
            Console.WriteLine($"Copied days {stopwatch.Elapsed.TotalSeconds} s.");
            Sideways.Sync.CopySplits(Database.DbFile, rebuilt);
            Console.WriteLine($"Copied splits {stopwatch.Elapsed.TotalSeconds} s.");
            Sideways.Sync.CopyDividends(Database.DbFile, rebuilt);
            Console.WriteLine($"Copied dividends {stopwatch.Elapsed.TotalSeconds} s.");
            Sideways.Sync.CopyMinutes(Database.DbFile, rebuilt);
            Console.WriteLine($"Copied minutes {stopwatch.Elapsed.TotalSeconds} s.");
        }

        [TestCaseSource(nameof(AppSymbols))]
        public static void OneWayToFlash(string symbol)
        {
            Sideways.Sync.CopyDays(symbol, Database.DbFile, FlashDrive);
            Console.WriteLine("Copied days.");
            Sideways.Sync.CopySplits(symbol, Database.DbFile, FlashDrive);
            Console.WriteLine("Copied splits.");
            Sideways.Sync.CopyDividends(symbol, Database.DbFile, FlashDrive);
            Console.WriteLine("Copied dividends.");
            Sideways.Sync.CopyMinutes(symbol, Database.DbFile, FlashDrive);
        }

        [Test]
        public static void OneWayToFlash()
        {
            Sideways.Sync.CopyDays(Database.DbFile, FlashDrive);
            Console.WriteLine("Copied days.");
            Sideways.Sync.CopySplits(Database.DbFile, FlashDrive);
            Console.WriteLine("Copied splits.");
            Sideways.Sync.CopyDividends(Database.DbFile, FlashDrive);
            Console.WriteLine("Copied dividends.");
            Sideways.Sync.CopyMinutes(Database.DbFile, FlashDrive);
        }

        [TestCaseSource(nameof(FlashSymbols))]
        public static void OneWayToApp(string symbol)
        {
            Sideways.Sync.CopyDays(symbol, FlashDrive, Database.DbFile);
            Console.WriteLine("Copied days.");
            Sideways.Sync.CopySplits(symbol, FlashDrive, Database.DbFile);
            Console.WriteLine("Copied splits.");
            Sideways.Sync.CopyDividends(symbol, FlashDrive, Database.DbFile);
            Console.WriteLine("Copied dividends.");
            Sideways.Sync.CopyMinutes(symbol, FlashDrive, Database.DbFile);
        }

        [Test]
        public static void OneWayToApp()
        {
            Sideways.Sync.CopyDays(FlashDrive, Database.DbFile);
            Console.WriteLine("Copied days.");
            Sideways.Sync.CopySplits(FlashDrive, Database.DbFile);
            Console.WriteLine("Copied splits.");
            Sideways.Sync.CopyDividends(FlashDrive, Database.DbFile);
            Console.WriteLine("Copied dividends.");
            Sideways.Sync.CopyMinutes(FlashDrive, Database.DbFile);
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

        private static void CopyDays(string symbol, FileInfo source, FileInfo target)
        {
            Console.WriteLine($"Copy days from {source} to {target}.");
            Sideways.Sync.CopyDays(symbol, source, target);
            Sideways.Sync.CopySplits(symbol, source, target);
            Sideways.Sync.CopyDividends(symbol, source, target);
        }

        private static IEnumerable<string> AppSymbols() => File.Exists(FlashDrive.FullName) ? Database.ReadSymbols() : Enumerable.Empty<string>();

        private static IEnumerable<string> FlashSymbols() => File.Exists(FlashDrive.FullName) ? Database.ReadSymbols(FlashDrive) : Enumerable.Empty<string>();

        private static IEnumerable<Diff> Diffs()
        {
            if (File.Exists(FlashDrive.FullName))
            {
                var dbDays = Database.DayRanges(Database.DbFile);
                var dbMinutes = Database.MinuteRanges(Database.DbFile);

                var flashDays = Database.DayRanges(FlashDrive);
                var flashMinutes = Database.MinuteRanges(FlashDrive);

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
