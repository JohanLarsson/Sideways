namespace Sideways
{
    using System;
    using System.IO;

    using Microsoft.Data.Sqlite;

    public static class Sync
    {
        public static void CopyAll(FileInfo source, FileInfo target)
        {
            CopyDays(source, target);
            CopySplits(source, target);
            CopyDividends(source, target);
            CopyMinutes(source, target);

            CopyAnnualEarnings(source, target);
            CopyQuarterlyEarnings(source, target);

            CopyListings(source, target);
        }

        public static void CopyAll(string symbol, FileInfo source, FileInfo target)
        {
            CopyDays(symbol, source, target);
            CopySplits(symbol, source, target);
            CopyDividends(symbol, source, target);
            CopyMinutes(symbol, source, target);

            CopyAnnualEarnings(symbol, source, target);
            CopyQuarterlyEarnings(symbol, source, target);
        }

        public static void CopyDays(FileInfo source, FileInfo target)
        {
            using var sourceConnection = Database.CreateConnection(source);
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
            using var sourceConnection = Database.CreateConnection(source);
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
            using var sourceConnection = Database.CreateConnection(source);
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, date, coefficient FROM splits" +
                "  ORDER BY symbol, date ASC",
                sourceConnection);
            using var reader = select.ExecuteReader();
            WriteSplits(reader, target);
        }

        public static void CopySplits(string symbol, FileInfo source, FileInfo target)
        {
            using var sourceConnection = Database.CreateConnection(source);
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
            using var sourceConnection = Database.CreateConnection(source);
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, date, dividend FROM dividends" +
                "  ORDER BY symbol, date ASC",
                sourceConnection);
            using var reader = select.ExecuteReader();
            WriteDividends(reader, target);
        }

        public static void CopyDividends(string symbol, FileInfo source, FileInfo target)
        {
            using var sourceConnection = Database.CreateConnection(source);
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
            using var sourceConnection = Database.CreateConnection(source);
            sourceConnection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, time, open, high, low, close, volume FROM minutes" +
                "  ORDER BY symbol, time ASC",
                sourceConnection);
            using var reader = select.ExecuteReader();
            WriteMinutes(reader, target);
        }

        public static void CopyMinutes(string symbol, FileInfo source, FileInfo target)
        {
            using var sourceConnection = Database.CreateConnection(source);
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

        public static void CopyListings(FileInfo source, FileInfo target)
        {
            using var connection = Database.CreateConnection(source);
            connection.Open();

            using var select = new SqliteCommand(
                "SELECT symbol, name, exchange, asset_type, ipo_date, delisting_date FROM listings",
                connection);
            using var reader = select.ExecuteReader();
            WriteListings(reader, target);

            static void WriteListings(SqliteDataReader reader, FileInfo target)
            {
                using var targetConnection = Database.CreateConnection(target);
                targetConnection.Open();

                using var targetTransaction = targetConnection.BeginTransaction();
                using var insert = targetConnection.CreateCommand();
                insert.CommandText =
                    "INSERT INTO listings (symbol, name, exchange, asset_type, ipo_date, delisting_date) VALUES (@symbol, @name, @exchange, @asset_type, @ipo_date, @delisting_date)" +
                    "  ON CONFLICT(symbol) DO NOTHING";
                insert.Prepare();

                while (reader.Read())
                {
                    insert.Parameters.Clear();
                    insert.Parameters.AddWithValue("@symbol", reader.GetString(0).ToUpperInvariant());
                    insert.Parameters.AddWithValue("@name", reader.IsDBNull(1) ? DBNull.Value : reader.GetString(1));
                    insert.Parameters.AddWithValue("@exchange", reader.GetString(2));
                    insert.Parameters.AddWithValue("@asset_type", reader.GetString(3));
                    insert.Parameters.AddWithValue("@ipo_date", reader.GetInt64(4));
                    insert.Parameters.AddWithValue("@delisting_date", reader.IsDBNull(5) ? DBNull.Value : reader.GetInt64(5));
                    insert.ExecuteNonQuery();
                }

                targetTransaction.Commit();
            }
        }

        public static void CopyAnnualEarnings(FileInfo source, FileInfo target)
        {
            using var connection = Database.CreateConnection(source);
            connection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, fiscal_date_ending, reported_eps FROM annual_earnings" +
                "  ORDER BY symbol, fiscal_date_ending ASC",
                connection);
            using var reader = select.ExecuteReader();
            WriteAnnualEarnings(reader, target);
        }

        public static void CopyAnnualEarnings(string symbol, FileInfo source, FileInfo target)
        {
            using var connection = Database.CreateConnection(source);
            connection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, fiscal_date_ending, reported_eps FROM annual_earnings" +
                " WHERE symbol = @symbol" +
                "  ORDER BY fiscal_date_ending ASC",
                connection);
            select.Parameters.AddWithValue("@symbol", symbol);
            using var reader = select.ExecuteReader();
            WriteAnnualEarnings(reader, target);
        }

        public static void CopyQuarterlyEarnings(FileInfo source, FileInfo target)
        {
            using var connection = Database.CreateConnection(source);
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT symbol, fiscal_date_ending, reported_date, reported_eps, estimated_eps FROM quarterly_earnings" +
                "  ORDER BY symbol, fiscal_date_ending ASC",
                connection);
            using var reader = command.ExecuteReader();
            WriteQuarterlyEarnings(reader, target);
        }

        public static void CopyQuarterlyEarnings(string symbol, FileInfo source, FileInfo target)
        {
            using var connection = Database.CreateConnection(source);
            connection.Open();
            using var select = new SqliteCommand(
                "SELECT symbol, fiscal_date_ending, reported_date, reported_eps, estimated_eps FROM quarterly_earnings" +
                " WHERE symbol = @symbol" +
                "  ORDER BY fiscal_date_ending ASC",
                connection);
            select.Parameters.AddWithValue("@symbol", symbol);
            using var reader = select.ExecuteReader();
            WriteQuarterlyEarnings(reader, target);
        }

        private static void WriteDays(SqliteDataReader reader, FileInfo target)
        {
            using var targetConnection = Database.CreateConnection(target);
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
                insert.Parameters.AddWithValue("@symbol", reader.GetString(0).ToUpperInvariant());
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
            using var targetConnection = Database.CreateConnection(target);
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText = "INSERT INTO splits (symbol, date, coefficient) VALUES (@symbol, @date, @coefficient)" +
                                 "  ON CONFLICT(symbol, date) DO NOTHING";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", reader.GetString(0).ToUpperInvariant());
                insert.Parameters.AddWithValue("@date", reader.GetValue(1));
                insert.Parameters.AddWithValue("@coefficient", reader.GetValue(2));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }

        private static void WriteDividends(SqliteDataReader reader, FileInfo target)
        {
            using var targetConnection = Database.CreateConnection(target);
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText = "INSERT INTO dividends (symbol, date, dividend) VALUES (@symbol, @date, @dividend)" +
                                 "  ON CONFLICT(symbol, date) DO NOTHING";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", reader.GetString(0).ToUpperInvariant());
                insert.Parameters.AddWithValue("@date", reader.GetValue(1));
                insert.Parameters.AddWithValue("@dividend", reader.GetValue(2));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }

        private static void WriteMinutes(SqliteDataReader reader, FileInfo target)
        {
            using var targetConnection = Database.CreateConnection(target);
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
                insert.Parameters.AddWithValue("@symbol", reader.GetString(0).ToUpperInvariant());
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

        private static void WriteAnnualEarnings(SqliteDataReader reader, FileInfo target)
        {
            using var targetConnection = Database.CreateConnection(target);
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText =
                "INSERT INTO annual_earnings (symbol, fiscal_date_ending, reported_eps) VALUES (@symbol, @fiscal_date_ending, @reported_eps)" +
                "  ON CONFLICT(symbol, fiscal_date_ending) DO NOTHING";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", reader.GetString(0).ToUpperInvariant());
                insert.Parameters.AddWithValue("@fiscal_date_ending", reader.GetInt64(1));
                insert.Parameters.AddWithValue("@reported_eps", reader.GetFloat(2));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }

        static void WriteQuarterlyEarnings(SqliteDataReader reader, FileInfo target)
        {
            using var targetConnection = Database.CreateConnection(target);
            targetConnection.Open();

            using var targetTransaction = targetConnection.BeginTransaction();
            using var insert = targetConnection.CreateCommand();
            insert.CommandText =
                "INSERT INTO quarterly_earnings (symbol, fiscal_date_ending, reported_date, reported_eps, estimated_eps) VALUES (@symbol, @fiscal_date_ending, @reported_date, @reported_eps, @estimated_eps)" +
                "  ON CONFLICT(symbol, fiscal_date_ending) DO NOTHING";
            insert.Prepare();

            while (reader.Read())
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", reader.GetString(0).ToUpperInvariant());
                insert.Parameters.AddWithValue("@fiscal_date_ending", reader.GetInt64(1));
                insert.Parameters.AddWithValue("@reported_date", reader.GetInt64(2));
                insert.Parameters.AddWithValue("@reported_eps", reader.GetFloat(3));
                insert.Parameters.AddWithValue("@estimated_eps", reader.IsDBNull(4) ? DBNull.Value : reader.GetFloat(4));
                insert.ExecuteNonQuery();
            }

            targetTransaction.Commit();
        }
    }
}
