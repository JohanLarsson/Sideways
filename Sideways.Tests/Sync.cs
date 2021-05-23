namespace Sideways.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;

    using NUnit.Framework;

    public static class Sync
    {
        private static readonly FileInfo FlashDrive = new("D:\\Database.sqlite3");

        [Explicit]
        [TestCaseSource(nameof(AllSymbols))]
        public static void OneWayToFlash(Diff diff)
        {
            if (diff.AppDays != diff.FlashDays)
            {
                CopyDays(diff.Symbol, diff.App, diff.Flash);
            }

            if (diff.AppMinutes != diff.FlashMinutes)
            {
                Sideways.Sync.CopyMinutes(diff.Symbol, diff.App, diff.Flash);
            }
        }

        [Explicit]
        [TestCaseSource(nameof(AllSymbols))]
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

        private static IEnumerable<Diff> AllSymbols()
        {
            if (File.Exists(FlashDrive.FullName))
            {
                var dbDays = Sideways.Sync.ReportDays(Database.DbFile);
                var dbMinutes = Sideways.Sync.ReportMinutes(Database.DbFile);

                var flashDays = Sideways.Sync.ReportDays(FlashDrive);
                var flashMinutes = Sideways.Sync.ReportMinutes(FlashDrive);

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
