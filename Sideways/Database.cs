namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;

    using Microsoft.Data.Sqlite;

    using Sideways.AlphaVantage;

    public static class Database
    {
        public static readonly FileInfo DbFile = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Database.sqlite3"));

        public static ImmutableArray<string> ReadSymbols(FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT DISTINCT symbol FROM days",
                connection);
            using var reader = command.ExecuteReader();
            var builder = ImmutableArray.CreateBuilder<string>();
            while (reader.Read())
            {
                builder.Add(reader.GetString(0));
            }

            return builder.ToImmutable();
        }

        public static DescendingCandles ReadDays(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT date, open, high, low, close, volume FROM days" +
                "  WHERE symbol = @symbol" +
                "  ORDER BY date DESC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            using var reader = command.ExecuteReader();
            return ReadCandles(reader);
        }

        public static DescendingCandles ReadDays(string symbol, DateTimeOffset from, DateTimeOffset to, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT date, open, high, low, close, volume FROM days" +
                "  WHERE symbol = @symbol AND date BETWEEN @from AND @to" +
                "  ORDER BY date DESC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            command.Parameters.AddWithValue("@from", from.ToUnixTimeSeconds());
            command.Parameters.AddWithValue("@to", to.ToUnixTimeSeconds());
            using var reader = command.ExecuteReader();
            return ReadCandles(reader);
        }

        public static DescendingCandles ReadMinutes(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT time, open, high, low, close, volume FROM minutes" +
                " WHERE symbol = @symbol" +
                " ORDER BY time DESC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            using var reader = command.ExecuteReader();
            return ReadCandles(reader);
        }

        public static DescendingCandles ReadMinutes(string symbol, DateTimeOffset from, DateTimeOffset to, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT time, open, high, low, close, volume FROM minutes" +
                "  WHERE symbol = @symbol AND time BETWEEN @from AND @to" +
                "  ORDER BY time DESC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            command.Parameters.AddWithValue("@from", from.ToUnixTimeSeconds());
            command.Parameters.AddWithValue("@to", to.ToUnixTimeSeconds());
            using var reader = command.ExecuteReader();
            return ReadCandles(reader);
        }

        public static DescendingSplits ReadSplits(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT date, coefficient FROM splits" +
                " WHERE symbol = @symbol" +
                " ORDER BY date DESC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            using var reader = command.ExecuteReader();
            var builder = DescendingSplits.CreateBuilder();
            while (reader.Read())
            {
                builder.Add(
                    new Split(
                        date: DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(0)),
                        coefficient: reader.GetDouble(1)));
            }

            return builder.Create();
        }

        public static void WriteDays(string symbol, IEnumerable<AdjustedCandle> candles, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();

            using var transaction = connection.BeginTransaction();

            using var insertDays = connection.CreateCommand();
            insertDays.CommandText = "INSERT INTO days (symbol, date, open, high, low, close, volume) VALUES (@symbol, @date, @open, @high, @low, @close, @volume)" +
                                     "  ON CONFLICT(symbol, date) DO UPDATE SET" +
                                     "    open = excluded.open," +
                                     "    high = excluded.high," +
                                     "    low = excluded.low," +
                                     "    close = excluded.close," +
                                     "    volume = excluded.volume";
            insertDays.Prepare();

            using var insertSplits = connection.CreateCommand();
            insertSplits.CommandText = "INSERT INTO splits (symbol, date, coefficient) VALUES (@symbol, @date, @coefficient)" +
                                       "  ON CONFLICT(symbol, date) DO UPDATE SET" +
                                       "    coefficient = excluded.coefficient";
            insertSplits.Prepare();

            using var insertDividends = connection.CreateCommand();
            insertDividends.CommandText = "INSERT INTO dividends (symbol, date, dividend) VALUES (@symbol, @date, @dividend)" +
                                          "  ON CONFLICT(symbol, date) DO UPDATE SET" +
                                          "    dividend = excluded.dividend";
            insertDividends.Prepare();

            foreach (var candle in candles)
            {
                insertDays.Parameters.Clear();
                insertDays.Parameters.AddWithValue("@symbol", symbol);
                insertDays.Parameters.AddWithValue("@date", candle.Time.ToUnixTimeSeconds());
                insertDays.Parameters.AddWithValue("@open", candle.Open.AsInt());
                insertDays.Parameters.AddWithValue("@high", candle.High.AsInt());
                insertDays.Parameters.AddWithValue("@low", candle.Low.AsInt());
                insertDays.Parameters.AddWithValue("@close", candle.Close.AsInt());
                insertDays.Parameters.AddWithValue("@volume", candle.Volume);
                insertDays.ExecuteNonQuery();

                if (candle.SplitCoefficient is not 1 and not 0)
                {
                    insertSplits.Parameters.Clear();
                    insertSplits.Parameters.AddWithValue("@symbol", symbol);
                    insertSplits.Parameters.AddWithValue("@date", candle.Time.ToUnixTimeSeconds());
                    insertSplits.Parameters.AddWithValue("@coefficient", candle.SplitCoefficient);
                    insertSplits.ExecuteNonQuery();
                }

                if (candle.SplitCoefficient is not 0)
                {
                    insertDividends.Parameters.Clear();
                    insertDividends.Parameters.AddWithValue("@symbol", symbol);
                    insertDividends.Parameters.AddWithValue("@date", candle.Time.ToUnixTimeSeconds());
                    insertDividends.Parameters.AddWithValue("@dividend", candle.Dividend);
                    insertDividends.ExecuteNonQuery();
                }
            }

            transaction.Commit();
        }

        public static void WriteMinutes(string symbol, IEnumerable<Candle> candles, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();

            using var transaction = connection.BeginTransaction();

            using var insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO minutes (symbol, time, open, high, low, close, volume) VALUES (@symbol, @time, @open, @high, @low, @close, @volume)" +
                                 "  ON CONFLICT(symbol, time) DO UPDATE SET" +
                                 "    open = excluded.open," +
                                 "    high = excluded.high," +
                                 "    low = excluded.low," +
                                 "    close = excluded.close," +
                                 "    volume = excluded.volume";
            insert.Prepare();

            foreach (var candle in candles)
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", symbol);
                insert.Parameters.AddWithValue("@time", candle.Time.ToUnixTimeSeconds());
                insert.Parameters.AddWithValue("@open", candle.Open.AsInt());
                insert.Parameters.AddWithValue("@high", candle.High.AsInt());
                insert.Parameters.AddWithValue("@low", candle.Low.AsInt());
                insert.Parameters.AddWithValue("@close", candle.Close.AsInt());
                insert.Parameters.AddWithValue("@volume", candle.Volume);
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        private static DescendingCandles ReadCandles(SqliteDataReader reader)
        {
            var builder = DescendingCandles.CreateBuilder();
            while (reader.Read())
            {
                builder.Add(
                    new Candle(
                        time: DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(0)),
                        open: Float(reader.GetInt32(1)),
                        high: Float(reader.GetInt32(2)),
                        low: Float(reader.GetInt32(3)),
                        close: Float(reader.GetInt32(4)),
                        volume: reader.GetInt32(5)));

                static float Float(int i) => (float)Math.Round(0.01 * i, 2);
            }

            return builder.Create();
        }

        private static int AsInt(this float f) => (int)Math.Round(f * 100);

        private static string Source(FileInfo file)
        {
            if (!Directory.Exists(file.Directory!.FullName))
            {
                Directory.CreateDirectory(file.Directory.FullName);
            }

            if (!File.Exists(file.FullName))
            {
                using var connection = new SqliteConnection($"Data Source={file}");
                connection.Open();
                using var transaction = connection.BeginTransaction();
                using var command = connection.CreateCommand();
                command.CommandText =
                    @"CREATE TABLE IF NOT EXISTS days(
                        symbol TEXT NOT NULL,
                        date INTEGER NOT NULL,
                        open INTEGER NOT NULL,
                        high INTEGER NOT NULL,
                        low INTEGER NOT NULL,
                        close INTEGER NOT NULL,
                        volume INTEGER NOT NULL,
                        PRIMARY KEY(symbol, date))";
                command.ExecuteNonQuery();

                command.CommandText =
                   @"CREATE TABLE IF NOT EXISTS minutes(
                        symbol TEXT NOT NULL,
                        time INTEGER NOT NULL,
                        open INTEGER NOT NULL,
                        high INTEGER NOT NULL,
                        low INTEGER NOT NULL,
                        close INTEGER NOT NULL,
                        volume INTEGER NOT NULL,
                        PRIMARY KEY(symbol, time))";
                command.ExecuteNonQuery();

                command.CommandText =
                    @"CREATE TABLE IF NOT EXISTS splits(
                        symbol TEXT NOT NULL,
                        date INTEGER NOT NULL,
                        coefficient REAL NOT NULL,
                        PRIMARY KEY(symbol, date))";
                command.ExecuteNonQuery();

                command.CommandText =
                    @"CREATE TABLE IF NOT EXISTS dividends(
                        symbol TEXT NOT NULL,
                        date INTEGER NOT NULL,
                        dividend REAL NOT NULL,
                        PRIMARY KEY(symbol, date))";
                command.ExecuteNonQuery();

                transaction.Commit();
            }

            return file.FullName;
        }
    }
}
