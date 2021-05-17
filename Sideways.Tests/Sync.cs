namespace Sideways.Tests
{
    using System.IO;

    using NUnit.Framework;

    public static class Sync
    {
        [TestCaseSource(typeof(Download), nameof(Download.Symbols))]
        public static void Copy(string symbol)
        {
            var source = new FileInfo("D:\\Database.sqlite3");
            var target = Database.DbFile;
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
    }
}
