namespace Sideways
{
    using System.IO;
    using Microsoft.Data.Sqlite;

    public static class Sync
    {
        public static long CountDays(string symbol, FileInfo file)
        {
            using var connection = new SqliteConnection($"Data Source={file.FullName}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT COUNT(*) FROM days" +
                " WHERE symbol = @symbol",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            return (long)command.ExecuteScalar();
        }

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

        public static void CopyDays(string symbol, FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT date, open, high, low, close, volume FROM days" +
                " WHERE symbol = @symbol" +
                " ORDER BY date DESC",
                sourceConnection);
            select.Parameters.AddWithValue("@symbol", symbol);
            using var reader = select.ExecuteReader();

            using var targetConnection = new SqliteConnection($"Data Source={target.FullName}");
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText = "INSERT INTO days (symbol, date, open, high, low, close, volume) VALUES (@symbol, @date, @open, @high, @low, @close, @volume)" +
                                 "  ON CONFLICT(symbol, date) DO UPDATE SET" +
                                 "    open = excluded.open," +
                                 "    high = excluded.high," +
                                 "    low = excluded.low," +
                                 "    close = excluded.close," +
                                 "    volume = excluded.volume";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", symbol);
                insert.Parameters.AddWithValue("@date", reader.GetValue(0));
                insert.Parameters.AddWithValue("@open", reader.GetValue(1));
                insert.Parameters.AddWithValue("@high", reader.GetValue(2));
                insert.Parameters.AddWithValue("@low", reader.GetValue(3));
                insert.Parameters.AddWithValue("@close", reader.GetValue(4));
                insert.Parameters.AddWithValue("@volume", reader.GetValue(5));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }

        public static void CopySplits(string symbol, FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT date, coefficient FROM splits" +
                " WHERE symbol = @symbol" +
                " ORDER BY date DESC",
                sourceConnection);
            select.Parameters.AddWithValue("@symbol", symbol);
            using var reader = select.ExecuteReader();

            using var targetConnection = new SqliteConnection($"Data Source={target.FullName}");
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText = "INSERT INTO splits (symbol, date, coefficient) VALUES (@symbol, @date, @coefficient)" +
                                 "  ON CONFLICT(symbol, date) DO UPDATE SET" +
                                 "    coefficient = excluded.coefficient";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", symbol);
                insert.Parameters.AddWithValue("@date", reader.GetValue(0));
                insert.Parameters.AddWithValue("@coefficient", reader.GetValue(1));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }

        public static void CopyDividends(string symbol, FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT date, dividend FROM dividends" +
                " WHERE symbol = @symbol" +
                " ORDER BY date DESC",
                sourceConnection);
            select.Parameters.AddWithValue("@symbol", symbol);
            using var reader = select.ExecuteReader();

            using var targetConnection = new SqliteConnection($"Data Source={target.FullName}");
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText = "INSERT INTO dividends (symbol, date, dividend) VALUES (@symbol, @date, @dividend)" +
                                 "  ON CONFLICT(symbol, date) DO UPDATE SET" +
                                 "    dividend = excluded.dividend";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", symbol);
                insert.Parameters.AddWithValue("@date", reader.GetValue(0));
                insert.Parameters.AddWithValue("@dividend", reader.GetValue(1));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }

        public static void CopyMinutes(string symbol, FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT time, open, high, low, close, volume FROM minutes" +
                " WHERE symbol = @symbol" +
                " ORDER BY time DESC",
                sourceConnection);
            select.Parameters.AddWithValue("@symbol", symbol);
            using var reader = select.ExecuteReader();

            using var targetConnection = new SqliteConnection($"Data Source={target.FullName}");
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText = "INSERT INTO minutes (symbol, time, open, high, low, close, volume) VALUES (@symbol, @time, @open, @high, @low, @close, @volume)" +
                                 "  ON CONFLICT(symbol, time) DO UPDATE SET" +
                                 "    open = excluded.open," +
                                 "    high = excluded.high," +
                                 "    low = excluded.low," +
                                 "    close = excluded.close," +
                                 "    volume = excluded.volume";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", symbol);
                insert.Parameters.AddWithValue("@time", reader.GetValue(0));
                insert.Parameters.AddWithValue("@open", reader.GetValue(1));
                insert.Parameters.AddWithValue("@high", reader.GetValue(2));
                insert.Parameters.AddWithValue("@low", reader.GetValue(3));
                insert.Parameters.AddWithValue("@close", reader.GetValue(4));
                insert.Parameters.AddWithValue("@volume", reader.GetValue(5));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }
    }
}
