namespace Sideways
{
    using System.IO;

    using Microsoft.Data.Sqlite;

    public static class Sync
    {
        public static void CopyDays(FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, date, open, high, low, close, volume FROM days" +
                "  ORDER BY symbol, date ASC",
                sourceConnection);
            using var reader = select.ExecuteReader();
            WriteDays(reader, target);
        }

        public static void CopyDays(string symbol, FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, date, open, high, low, close, volume FROM days" +
                " WHERE symbol = @symbol" +
                "  ORDER BY date ASC",
                sourceConnection);
            select.Parameters.AddWithValue("@symbol", symbol);
            using var reader = select.ExecuteReader();
            WriteDays(reader, target);
        }

        public static void CopySplits(FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, date, coefficient FROM splits" +
                "  ORDER BY date ASC",
                sourceConnection);
            using var reader = select.ExecuteReader();
            WriteSplits(reader, target);
        }

        public static void CopySplits(string symbol, FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, date, coefficient FROM splits" +
                " WHERE symbol = @symbol" +
                "  ORDER BY date ASC",
                sourceConnection);
            select.Parameters.AddWithValue("@symbol", symbol);
            using var reader = select.ExecuteReader();
            WriteSplits(reader, target);
        }

        public static void CopyDividends(FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, date, dividend FROM dividends" +
                "  ORDER BY date ASC",
                sourceConnection);
            using var reader = select.ExecuteReader();
            WriteDividends(reader, target);
        }

        public static void CopyDividends(string symbol, FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, date, dividend FROM dividends" +
                " WHERE symbol = @symbol" +
                "  ORDER BY date ASC",
                sourceConnection);
            select.Parameters.AddWithValue("@symbol", symbol);
            using var reader = select.ExecuteReader();
            WriteDividends(reader, target);
        }

        public static void CopyMinutes(FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, time, open, high, low, close, volume FROM minutes" +
                "  ORDER BY time ASC",
                sourceConnection);
            using var reader = select.ExecuteReader();
            WriteMinutes(reader, target);
        }

        public static void CopyMinutes(string symbol, FileInfo source, FileInfo target)
        {
            using var sourceConnection = new SqliteConnection($"Data Source={source.FullName}");
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, time, open, high, low, close, volume FROM minutes" +
                " WHERE symbol = @symbol" +
                "  ORDER BY time ASC",
                sourceConnection);
            select.Parameters.AddWithValue("@symbol", symbol);
            using var reader = select.ExecuteReader();
            WriteMinutes(reader, target);
        }

        private static void WriteDays(SqliteDataReader reader, FileInfo target)
        {
            using var targetConnection = new SqliteConnection($"Data Source={target.FullName}");
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText =
                "INSERT INTO days (symbol, date, open, high, low, close, volume) VALUES (@symbol, @date, @open, @high, @low, @close, @volume)" +
                "  ON CONFLICT(symbol, date) DO NOTHING";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", reader.GetValue(0));
                insert.Parameters.AddWithValue("@date", reader.GetValue(1));
                insert.Parameters.AddWithValue("@open", reader.GetValue(2));
                insert.Parameters.AddWithValue("@high", reader.GetValue(3));
                insert.Parameters.AddWithValue("@low", reader.GetValue(4));
                insert.Parameters.AddWithValue("@close", reader.GetValue(5));
                insert.Parameters.AddWithValue("@volume", reader.GetValue(6));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }

        private static void WriteSplits(SqliteDataReader reader, FileInfo target)
        {
            using var targetConnection = new SqliteConnection($"Data Source={target.FullName}");
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText = "INSERT INTO splits (symbol, date, coefficient) VALUES (@symbol, @date, @coefficient)" +
                                 "  ON CONFLICT(symbol, date) DO NOTHING";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", reader.GetValue(0));
                insert.Parameters.AddWithValue("@date", reader.GetValue(1));
                insert.Parameters.AddWithValue("@coefficient", reader.GetValue(2));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }

        private static void WriteDividends(SqliteDataReader reader, FileInfo target)
        {
            using var targetConnection = new SqliteConnection($"Data Source={target.FullName}");
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText = "INSERT INTO dividends (symbol, date, dividend) VALUES (@symbol, @date, @dividend)" +
                                 "  ON CONFLICT(symbol, date) DO NOTHING";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", reader.GetValue(0));
                insert.Parameters.AddWithValue("@date", reader.GetValue(1));
                insert.Parameters.AddWithValue("@dividend", reader.GetValue(2));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }

        private static void WriteMinutes(SqliteDataReader reader, FileInfo target)
        {
            using var targetConnection = new SqliteConnection($"Data Source={target.FullName}");
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText =
                "INSERT INTO minutes (symbol, time, open, high, low, close, volume) VALUES (@symbol, @time, @open, @high, @low, @close, @volume)" +
                "  ON CONFLICT(symbol, time) DO NOTHING";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", reader.GetValue(0));
                insert.Parameters.AddWithValue("@time", reader.GetValue(1));
                insert.Parameters.AddWithValue("@open", reader.GetValue(2));
                insert.Parameters.AddWithValue("@high", reader.GetValue(3));
                insert.Parameters.AddWithValue("@low", reader.GetValue(4));
                insert.Parameters.AddWithValue("@close", reader.GetValue(5));
                insert.Parameters.AddWithValue("@volume", reader.GetValue(6));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }
    }
}
