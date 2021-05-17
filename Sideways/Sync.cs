namespace Sideways
{
    using System.IO;
    using Microsoft.Data.Sqlite;

    public static class Sync
    {
        public static long CountMinutes(string symbol, FileInfo file)
        {
            using var connection = new SqliteConnection($"Data Source={file.FullName}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT COUNT(*) FROM minutes" +
                " WHERE symbol = @symbol",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            return (long)command.ExecuteScalar();
        }

        public static void Copy(string symbol, FileInfo source, FileInfo target)
        {
            Database.WriteMinutes(symbol, Database.ReadMinutes(symbol, source), target);
        }
    }
}
