namespace Sideways.Tests
{
    using System.IO;

    using NUnit.Framework;

    public static class Sync
    {
        [TestCaseSource(typeof(Download), nameof(Download.Symbols))]
        public static void Copy(string symbol)
        {
            var target = new FileInfo("D:\\Database.sqlite3");
            if (File.Exists(target.FullName))
            {
                if (Sideways.Sync.CountMinutes(symbol, Database.DbFile) > Sideways.Sync.CountMinutes(symbol, target))
                {
                    Sideways.Sync.CopyMinutes(symbol, Database.DbFile, target);
                }

                if (Sideways.Sync.CountDays(symbol, Database.DbFile) > Sideways.Sync.CountDays(symbol, target))
                {
                    Sideways.Sync.CopyDays(symbol, Database.DbFile, target);
                }
            }
        }
    }
}
