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
                    Sideways.Sync.Copy(symbol, Database.DbFile, target);
                }
            }
        }
    }
}
