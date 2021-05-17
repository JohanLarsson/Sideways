namespace Sideways.Tests
{
    using System.IO;
    using NUnit.Framework;

    public static class Sync
    {
        [TestCaseSource(typeof(Download), nameof(Download.Symbols))]
        public static void Copy(string symbol)
        {
            var target =new FileInfo( "D:\\Database.sqlite3");
            if (File.Exists(target.FullName))
            {
                Database.Copy(symbol, Database.DbFile, target);
            }
        }
    }
}
