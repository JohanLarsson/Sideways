namespace Sideways.Tests
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using NUnit.Framework;

    public static class Sync
    {
        private static readonly FileInfo FlashDrive = new FileInfo("D:\\Database.sqlite3");

        [Explicit]
        [TestCaseSource(nameof(AllSymbols))]
        public static void OneWay(string symbol)
        {
            var source = Database.DbFile;
            var target = new FileInfo("D:\\Database.sqlite3");
            if (File.Exists(source.FullName) &&
                File.Exists(target.FullName))
            {
                if (Sideways.Sync.CountDays(symbol, source) > Sideways.Sync.CountDays(symbol, target))
                {
                    Sideways.Sync.CopyDays(symbol, source, target);
                    Sideways.Sync.CopySplits(symbol, source, target);
                    Sideways.Sync.CopyDividends(symbol, source, target);
                }

                if (Sideways.Sync.CountMinutes(symbol, source) > Sideways.Sync.CountMinutes(symbol, target))
                {
                    Sideways.Sync.CopyMinutes(symbol, source, target);
                }
            }
        }

        [Explicit]
        [TestCaseSource(nameof(AllSymbols))]
        public static void TwoWay(string symbol)
        {
            var x = Database.DbFile;
            var y = FlashDrive;
            if (File.Exists(x.FullName) &&
                File.Exists(y.FullName))
            {
                switch (Comparer<long>.Default.Compare(Sideways.Sync.CountDays(symbol, x), Sideways.Sync.CountDays(symbol, y)))
                {
                    case < 0:
                        CopyDays(symbol, y, x);
                        break;
                    case > 0:
                        CopyDays(symbol, x, y);
                        break;
                    case 0:
                        break;
                }

                switch (Comparer<long>.Default.Compare(Sideways.Sync.CountMinutes(symbol, x), Sideways.Sync.CountMinutes(symbol, y)))
                {
                    case < 0:
                        Sideways.Sync.CopyMinutes(symbol, y, x);
                        break;
                    case > 0:
                        Sideways.Sync.CopyMinutes(symbol, x, y);
                        break;
                    case 0:
                        break;
                }

                static void CopyDays(string symbol, FileInfo source, FileInfo target)
                {
                    Sideways.Sync.CopyDays(symbol, source, target);
                    Sideways.Sync.CopySplits(symbol, source, target);
                    Sideways.Sync.CopyDividends(symbol, source, target);
                }
            }
        }

        private static IEnumerable<string> AllSymbols() => Database.ReadSymbols(Database.DbFile).Concat(Database.ReadSymbols(FlashDrive)).Distinct();
    }
}
