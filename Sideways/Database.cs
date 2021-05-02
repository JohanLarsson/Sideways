namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Data.Sqlite;

    public static class Database
    {
        private const string Minutes = "MINUTES";
        private const string Weeks = "WEEKS";
        private const string Days = "DAYS";
        private const string AdjustedDays = "ADJUSTEDDAYS";
        private const string Hours = "HOURS";

        public static Task<ImmutableArray<Candle>> ReadWeeksAsync(string symbol)
        {
            return ReadAsync(symbol, Weeks);
        }

        public static Task<ImmutableArray<Candle>> ReadWeeksAsync(string symbol, DateTimeOffset from, DateTimeOffset to)
        {
            return ReadAsync(symbol, Weeks, from, to);
        }

        public static Task<ImmutableArray<Candle>> ReadWeeksAsync(string symbol, int n, DateTimeOffset to)
        {
            return ReadAsync(symbol, Weeks, n, to);
        }

        public static void WriteWeeks(string symbol, IEnumerable<Candle> candles)
        {
            Write(symbol, Weeks, candles);
        }

        public static Task<ImmutableArray<Candle>> ReadDaysAsync(string symbol)
        {
            return ReadAsync(symbol, Days);
        }

        public static Task<ImmutableArray<Candle>> ReadDaysAsync(string symbol, DateTimeOffset from, DateTimeOffset to)
        {
            return ReadAsync(symbol, Days, from, to);
        }

        public static Task<ImmutableArray<Candle>> ReadDaysAsync(string symbol, int n, DateTimeOffset to)
        {
            return ReadAsync(symbol, Days, n, to);
        }

        public static void WriteDays(string symbol, IEnumerable<Candle> candles)
        {
            Write(symbol, Days, candles);
        }

        public static Task<ImmutableArray<AdjustedCandle>> ReadAdjustedDaysAsync(string symbol)
        {
            return ReadAdjustedAsync(symbol, AdjustedDays);
        }

        public static void WriteAdjustedDays(string symbol, IEnumerable<AdjustedCandle> candles)
        {
            Write(symbol, AdjustedDays, candles);
        }

        public static Task<ImmutableArray<Candle>> ReadHoursAsync(string symbol)
        {
            return ReadAsync(symbol, Hours);
        }

        public static Task<ImmutableArray<Candle>> ReadHoursAsync(string symbol, DateTimeOffset from, DateTimeOffset to)
        {
            return ReadAsync(symbol, Hours, from, to);
        }

        public static Task<ImmutableArray<Candle>> ReadHoursAsync(string symbol, int n, DateTimeOffset to)
        {
            return ReadAsync(symbol, Hours, n, to);
        }

        public static void WriteHours(string symbol, IEnumerable<Candle> candles)
        {
            Write(symbol, Hours, candles);
        }

        public static Task<ImmutableArray<Candle>> ReadMinutesAsync(string symbol)
        {
            return ReadAsync(symbol, Minutes);
        }

        public static Task<ImmutableArray<Candle>> ReadMinutesAsync(string symbol, DateTimeOffset from, DateTimeOffset to)
        {
            return ReadAsync(symbol, Minutes, from, to);
        }

        public static Task<ImmutableArray<Candle>> ReadMinutesAsync(string symbol, int n, DateTimeOffset to)
        {
            return ReadAsync(symbol, Minutes, n, to);
        }

        public static void WriteMinutes(string symbol, IEnumerable<Candle> candles)
        {
            Write(symbol, Minutes, candles);
        }

        private static async Task<ImmutableArray<Candle>> ReadAsync(string symbol, string period)
        {
            await using var connection = new SqliteConnection($"Data Source={Source(symbol, period)}");
            await connection.OpenAsync().ConfigureAwait(false);
            CreateCandlesIfNotExists(connection);
            await using var command = new SqliteCommand("SELECT * FROM candles", connection);
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            return ReadCandles(reader);
        }

        private static async Task<ImmutableArray<AdjustedCandle>> ReadAdjustedAsync(string symbol, string period)
        {
            await using var connection = new SqliteConnection($"Data Source={Source(symbol, period)}");
            await connection.OpenAsync().ConfigureAwait(false);
            CreateAdjustedCandlesIfNotExists(connection);
            await using var command = new SqliteCommand("SELECT * FROM candles", connection);
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            return ReadAdjustedCandles(reader);
        }

        private static async Task<ImmutableArray<Candle>> ReadAsync(string symbol, string period, DateTimeOffset from, DateTimeOffset to)
        {
            await using var connection = new SqliteConnection($"Data Source={Source(symbol, period)}");
            await connection.OpenAsync().ConfigureAwait(false);
            CreateCandlesIfNotExists(connection);
            await using var command = new SqliteCommand($"SELECT * FROM candles\r\nWHERE date BETWEEN @from AND @to", connection);
            command.Parameters.AddWithValue("@from", from.ToUnixTimeSeconds());
            command.Parameters.AddWithValue("@to", to.ToUnixTimeSeconds());
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            return ReadCandles(reader);
        }

        private static async Task<ImmutableArray<Candle>> ReadAsync(string symbol, string period, int n, DateTimeOffset to)
        {
            await using var connection = new SqliteConnection($"Data Source={Source(symbol, period)}");
            await connection.OpenAsync().ConfigureAwait(false);
            CreateCandlesIfNotExists(connection);
            await using var command = new SqliteCommand($"SELECT * FROM candles\r\nWHERE date <= @to ORDER BY date DESC LIMIT @n", connection);
            command.Parameters.AddWithValue("@n", n);
            command.Parameters.AddWithValue("@to", to.ToUnixTimeSeconds());
            await using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
            return ReadCandles(reader);
        }

        private static ImmutableArray<Candle> ReadCandles(SqliteDataReader reader)
        {
            var builder = ImmutableArray.CreateBuilder<Candle>();
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

            return builder.ToImmutable();
        }

        private static ImmutableArray<AdjustedCandle> ReadAdjustedCandles(SqliteDataReader reader)
        {
            var builder = ImmutableArray.CreateBuilder<AdjustedCandle>();
            while (reader.Read())
            {
                builder.Add(
                    new AdjustedCandle(
                        time: DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(0)),
                        open: Float(reader.GetInt32(1)),
                        high: Float(reader.GetInt32(2)),
                        low: Float(reader.GetInt32(3)),
                        close: Float(reader.GetInt32(4)),
                        adjustedClose: Float(reader.GetInt32(5)),
                        volume: reader.GetInt32(6),
                        dividend: Float(reader.GetInt32(7)),
                        splitCoefficient: Float(reader.GetInt32(8))));

                static float Float(int i) => (float)Math.Round(0.01 * i, 2);
            }

            return builder.ToImmutable();
        }

        private static void Write(string symbol, string period, IEnumerable<Candle> candles)
        {
            using var connection = new SqliteConnection($"Data Source={Source(symbol, period)}");
            connection.Open();

            using var transaction = connection.BeginTransaction();
            CreateCandlesIfNotExists(connection, transaction);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO candles (date, open, high, low, close, volume) VALUES (@date, @open, @high, @low, @close, @volume)\r\n" +
                              "  ON CONFLICT(date) DO NOTHING";
            foreach (var candle in candles)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@date", candle.Time.ToUnixTimeSeconds());
                cmd.Parameters.AddWithValue("@open", Int(candle.Open));
                cmd.Parameters.AddWithValue("@high", Int(candle.High));
                cmd.Parameters.AddWithValue("@low", Int(candle.Low));
                cmd.Parameters.AddWithValue("@close", Int(candle.Close));
                cmd.Parameters.AddWithValue("@volume", candle.Volume);
                cmd.Prepare();
                cmd.ExecuteNonQuery();

                static int Int(float f) => (int)Math.Round(f * 100);
            }

            transaction.Commit();
        }

        private static void Write(string symbol, string period, IEnumerable<AdjustedCandle> candles)
        {
            using var connection = new SqliteConnection($"Data Source={Source(symbol, period)}");
            connection.Open();

            using var transaction = connection.BeginTransaction();
            CreateAdjustedCandlesIfNotExists(connection, transaction);

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "INSERT INTO candles (date, open, high, low, close, adjustedClose, volume, dividend, splitCoefficient) VALUES (@date, @open, @high, @low, @close, @adjustedClose, @volume, @dividend, @splitCoefficient)\r\n" +
                              "  ON CONFLICT(date) DO NOTHING";
            foreach (var candle in candles)
            {
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@date", candle.Time.ToUnixTimeSeconds());
                cmd.Parameters.AddWithValue("@open", Int(candle.Open));
                cmd.Parameters.AddWithValue("@high", Int(candle.High));
                cmd.Parameters.AddWithValue("@low", Int(candle.Low));
                cmd.Parameters.AddWithValue("@close", Int(candle.Close));
                cmd.Parameters.AddWithValue("@adjustedClose", Int(candle.AdjustedClose));
                cmd.Parameters.AddWithValue("@volume", candle.Volume);
                cmd.Parameters.AddWithValue("@dividend", Int(candle.Dividend));
                cmd.Parameters.AddWithValue("@splitCoefficient", Int(candle.SplitCoefficient));
                cmd.Prepare();
                cmd.ExecuteNonQuery();

                static int Int(float f) => (int)Math.Round(f * 100);
            }

            transaction.Commit();
        }

        private static string Source(string symbol, string period)
        {
            var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"Sideways\\Symbols\\{symbol}");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            return Path.Combine(dir, $"{period}.db");
        }

        private static void CreateCandlesIfNotExists(SqliteConnection connection, SqliteTransaction? transaction = null)
        {
            using var create = new SqliteCommand(@"CREATE TABLE IF NOT EXISTS candles(date INTEGER PRIMARY KEY, open INTEGER, high INTEGER, low INTEGER, close INTEGER, volume INTEGER)", connection, transaction);
            create.ExecuteNonQuery();
        }

        private static void CreateAdjustedCandlesIfNotExists(SqliteConnection connection, SqliteTransaction? transaction = null)
        {
            using var create = new SqliteCommand(@"CREATE TABLE IF NOT EXISTS candles(date INTEGER PRIMARY KEY, open INTEGER, high INTEGER, low INTEGER, close INTEGER, adjustedClose INTEGER, volume INTEGER, dividend INTEGER, splitCoefficient INTEGER)", connection, transaction);
            create.ExecuteNonQuery();
        }
    }
}
