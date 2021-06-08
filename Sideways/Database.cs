namespace Sideways
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;

    using Microsoft.Data.Sqlite;

    using Sideways.AlphaVantage;

    public static class Database
    {
        public static readonly FileInfo DbFile = new(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Database.sqlite3"));
        private static readonly ConcurrentDictionary<FileInfo, object?> Migrated = new(FullNameComparer.Default);

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

        public static SortedCandles ReadDays(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT date, open, high, low, close, volume FROM days" +
                "  WHERE symbol = @symbol" +
                "  ORDER BY date ASC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            using var reader = command.ExecuteReader();
            return ReadCandles(reader);
        }

        public static SortedCandles ReadDays(string symbol, DateTimeOffset from, DateTimeOffset to, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT date, open, high, low, close, volume FROM days" +
                "  WHERE symbol = @symbol AND date BETWEEN @from AND @to" +
                "  ORDER BY date ASC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            command.Parameters.AddWithValue("@from", from.ToUnixTimeSeconds());
            command.Parameters.AddWithValue("@to", to.ToUnixTimeSeconds());
            using var reader = command.ExecuteReader();
            return ReadCandles(reader);
        }

        public static ImmutableDictionary<string, TimeRange> DayRanges(FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT symbol, MIN(date), MAX(date) FROM days" +
                " GROUP BY symbol",
                connection);
            using var reader = command.ExecuteReader();
            var builder = ImmutableDictionary.CreateBuilder<string, TimeRange>();
            while (reader.Read())
            {
                builder.Add(
                    reader.GetString(0),
                    new TimeRange(
                        DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(1)),
                        DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2))));
            }

            return builder.ToImmutable();
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

            foreach (var candle in candles.OrderBy(x => x.Time))
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

        public static void WriteDays(string symbol, IEnumerable<Candle> candles, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();

            using var transaction = connection.BeginTransaction();
            using var insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO days (symbol, date, open, high, low, close, volume) VALUES (@symbol, @date, @open, @high, @low, @close, @volume)" +
                                 "  ON CONFLICT(symbol, date) DO NOTHING";
            insert.Prepare();

            foreach (var candle in candles.OrderBy(x => x.Time))
            {
                if (candle.Time.Hour != 0 ||
                    candle.Time.Minute != 0 ||
                    candle.Time.Second != 0)
                {
                    throw new InvalidOperationException("Expected time to be midnight");
                }

                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", symbol);
                insert.Parameters.AddWithValue("@date", candle.Time.ToUnixTimeSeconds());
                insert.Parameters.AddWithValue("@open", candle.Open.AsInt());
                insert.Parameters.AddWithValue("@high", candle.High.AsInt());
                insert.Parameters.AddWithValue("@low", candle.Low.AsInt());
                insert.Parameters.AddWithValue("@close", candle.Close.AsInt());
                insert.Parameters.AddWithValue("@volume", candle.Volume);
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public static SortedCandles ReadMinutes(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT time, open, high, low, close, volume FROM minutes" +
                " WHERE symbol = @symbol" +
                " ORDER BY time ASC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            using var reader = command.ExecuteReader();
            return ReadCandles(reader);
        }

        public static SortedCandles ReadMinutes(string symbol, DateTimeOffset from, DateTimeOffset to, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT time, open, high, low, close, volume FROM minutes" +
                "  WHERE symbol = @symbol AND time BETWEEN @from AND @to" +
                "  ORDER BY time ASC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            command.Parameters.AddWithValue("@from", from.ToUnixTimeSeconds());
            command.Parameters.AddWithValue("@to", to.ToUnixTimeSeconds());
            using var reader = command.ExecuteReader();
            return ReadCandles(reader);
        }

        public static DateTimeOffset? FirstMinute(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            return connection.ExecuteScalar(
                "SELECT time FROM minutes" +
                "  WHERE symbol = @symbol" +
                "  ORDER BY time ASC" +
                "  LIMIT 1",
                new SqliteParameter("@symbol", symbol)) is long s
                ? DateTimeOffset.FromUnixTimeSeconds(s)
                : null;
        }

        public static DateTimeOffset? LastMinute(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            return connection.ExecuteScalar(
                "SELECT time FROM minutes" +
                "  WHERE symbol = @symbol" +
                "  ORDER BY time DESC" +
                "  LIMIT 1",
                new SqliteParameter("@symbol", symbol)) is long s
                ? DateTimeOffset.FromUnixTimeSeconds(s)
                : null;
        }

        public static TimeRange MinuteRange(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            var parameter = new SqliteParameter("@symbol", symbol);

            if (Min() is long min &&
                Max() is long max)
            {
                return new TimeRange(
                    DateTimeOffset.FromUnixTimeSeconds(min),
                    DateTimeOffset.FromUnixTimeSeconds(max));
            }

            return default;

            object Min() => connection.ExecuteScalar(
                "SELECT time FROM minutes" +
                "  WHERE symbol = @symbol" +
                "  ORDER BY time ASC" +
                "  LIMIT 1",
                parameter);

            object Max() => connection.ExecuteScalar(
                "SELECT time FROM minutes" +
                "  WHERE symbol = @symbol" +
                "  ORDER BY time DESC" +
                "  LIMIT 1",
                parameter);
        }

        public static ImmutableDictionary<string, TimeRange> MinuteRanges(IEnumerable<string> symbols, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            var builder = ImmutableDictionary.CreateBuilder<string, TimeRange>();
            foreach (var symbol in symbols)
            {
                var parameter = new SqliteParameter("@symbol", symbol);

                if (Min() is long min &&
                    Max() is long max)
                {
                    builder.Add(
                        symbol,
                        new TimeRange(
                            DateTimeOffset.FromUnixTimeSeconds(min),
                            DateTimeOffset.FromUnixTimeSeconds(max)));
                }

                object Min() => connection.ExecuteScalar(
                    "SELECT time FROM minutes" +
                    "  WHERE symbol = @symbol" +
                    "  ORDER BY time ASC" +
                    "  LIMIT 1",
                    parameter);

                object Max() => connection.ExecuteScalar(
                    "SELECT time FROM minutes" +
                    "  WHERE symbol = @symbol" +
                    "  ORDER BY time DESC" +
                    "  LIMIT 1",
                    parameter);
            }

            return builder.ToImmutable();
        }

        public static ImmutableDictionary<string, TimeRange> MinuteRanges(FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT symbol, MIN(time), MAX(time) FROM minutes" +
                " GROUP BY symbol",
                connection);
            using var reader = command.ExecuteReader();
            var builder = ImmutableDictionary.CreateBuilder<string, TimeRange>();
            while (reader.Read())
            {
                builder.Add(
                    reader.GetString(0),
                    new TimeRange(
                        DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(1)),
                        DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(2))));
            }

            return builder.ToImmutable();
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

            foreach (var candle in candles.OrderBy(x => x.Time))
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

        public static SortedSplits ReadSplits(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT date, coefficient FROM splits" +
                " WHERE symbol = @symbol" +
                " ORDER BY date ASC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            using var reader = command.ExecuteReader();
            var builder = SortedSplits.CreateBuilder();
            while (reader.Read())
            {
                builder.Add(
                    new Split(
                        date: DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(0)),
                        coefficient: reader.GetDouble(1)));
            }

            return builder.Create();
        }

        public static ImmutableArray<Listing> ReadListings(FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();

            using var command = new SqliteCommand(
                "SELECT symbol, name, exchange, asset_type, ipo_date, delisting_date FROM listings",
                connection);
            using var reader = command.ExecuteReader();
            var builder = ImmutableArray.CreateBuilder<Listing>();
            while (reader.Read())
            {
                builder.Add(new Listing(
                    symbol: reader.GetString(0),
                    name: reader.IsDBNull(1) ? null : reader.GetString(1),
                    exchange: reader.GetString(2),
                    assetType: reader.GetString(3),
                    ipoDate: DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(4)),
                    delistingDate: reader.IsDBNull(5) ? null : DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(5))));
            }

            return builder.ToImmutable();
        }

        public static void WriteListings(IEnumerable<Listing> listings, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();

            using var transaction = connection.BeginTransaction();
            using var insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO listings (symbol, name, exchange, asset_type, ipo_date, delisting_date) VALUES (@symbol, @name, @exchange, @asset_type, @ipo_date, @delisting_date)" +
                                 "  ON CONFLICT(symbol) DO UPDATE SET" +
                                 "    name = excluded.name," +
                                 "    exchange = excluded.exchange," +
                                 "    asset_type = excluded.asset_type," +
                                 "    ipo_date = excluded.ipo_date," +
                                 "    delisting_date = excluded.delisting_date";
            insert.Prepare();

            foreach (var listing in listings)
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", listing.Symbol);
                insert.Parameters.AddWithValue("@name", (object?)listing.Name ?? DBNull.Value);
                insert.Parameters.AddWithValue("@exchange", listing.Exchange);
                insert.Parameters.AddWithValue("@asset_type", listing.AssetType);
                insert.Parameters.AddWithValue("@ipo_date", listing.IpoDate.ToUnixTimeSeconds());
#pragma warning disable CA1508 // Avoid dead conditional code
                insert.Parameters.AddWithValue("@delisting_date", (object?)listing.DelistingDate?.ToUnixTimeSeconds() ?? DBNull.Value);
#pragma warning restore CA1508 // Avoid dead conditional code
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public static ImmutableArray<AnnualEarning> ReadAnnualEarnings(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();

            using var command = new SqliteCommand(
                "SELECT fiscal_date_ending, reported_eps FROM annual_earnings" +
                " WHERE symbol = @symbol" +
                " ORDER BY fiscal_date_ending ASC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            using var reader = command.ExecuteReader();
            var builder = ImmutableArray.CreateBuilder<AnnualEarning>();
            while (reader.Read())
            {
                builder.Add(
                    new AnnualEarning(
                        fiscalDateEnding: DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(0)),
                        reportedEps: reader.GetFloat(1)));
            }

            return builder.ToImmutable();
        }

        public static void WriteAnnualEarnings(string symbol, IEnumerable<AnnualEarning> earnings, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();

            using var transaction = connection.BeginTransaction();
            using var insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO annual_earnings (symbol, fiscal_date_ending, reported_eps) VALUES (@symbol, @fiscal_date_ending, @reported_eps)" +
                                 "  ON CONFLICT(symbol, fiscal_date_ending) DO UPDATE SET" +
                                 "    symbol = excluded.symbol," +
                                 "    fiscal_date_ending = excluded.fiscal_date_ending," +
                                 "    reported_eps = excluded.reported_eps";
            insert.Prepare();

            foreach (var annualEarning in earnings)
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", symbol);
                insert.Parameters.AddWithValue("@fiscal_date_ending", annualEarning.FiscalDateEnding.ToUnixTimeSeconds());
                insert.Parameters.AddWithValue("@reported_eps", annualEarning.ReportedEPS);
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public static ImmutableArray<QuarterlyEarning> ReadQuarterlyEarnings(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();

            using var command = new SqliteCommand(
                "SELECT fiscal_date_ending, reported_date, reported_eps, estimated_eps FROM quarterly_earnings" +
                " WHERE symbol = @symbol" +
                " ORDER BY fiscal_date_ending ASC",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            using var reader = command.ExecuteReader();
            var builder = ImmutableArray.CreateBuilder<QuarterlyEarning>();
            while (reader.Read())
            {
                builder.Add(
                    new QuarterlyEarning(
                        fiscalDateEnding: DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(0)),
                        reportedDate: DateTimeOffset.FromUnixTimeSeconds(reader.GetInt64(1)),
                        reportedEps: reader.GetFloat(2),
                        estimatedEps: reader.IsDBNull(3) ? null : reader.GetFloat(3)));
            }

            return builder.ToImmutable();
        }

        public static void WriteQuarterlyEarnings(string symbol, IEnumerable<QuarterlyEarning> earnings, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();

            using var transaction = connection.BeginTransaction();
            using var insert = connection.CreateCommand();
            insert.CommandText = "INSERT INTO quarterly_earnings (symbol, fiscal_date_ending, reported_date, reported_eps, estimated_eps) VALUES (@symbol, @fiscal_date_ending, @reported_date, @reported_eps, @estimated_eps)" +
                                 "  ON CONFLICT(symbol, fiscal_date_ending) DO UPDATE SET" +
                                 "    symbol = excluded.symbol," +
                                 "    fiscal_date_ending = excluded.fiscal_date_ending," +
                                 "    reported_date = excluded.reported_date," +
                                 "    reported_eps = excluded.reported_eps," +
                                 "    estimated_eps = excluded.estimated_eps";
            insert.Prepare();

            foreach (var earning in earnings)
            {
                insert.Parameters.Clear();
                insert.Parameters.AddWithValue("@symbol", symbol);
                insert.Parameters.AddWithValue("@fiscal_date_ending", earning.FiscalDateEnding.ToUnixTimeSeconds());
                insert.Parameters.AddWithValue("@reported_date", earning.ReportedDate.ToUnixTimeSeconds());
                insert.Parameters.AddWithValue("@reported_eps", earning.ReportedEps);
                insert.Parameters.AddWithValue("@estimated_eps", earning.EstimatedEps is null ? DBNull.Value : earning.EstimatedEps);
                insert.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public static long CountDays(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT COUNT(*) FROM days" +
                " WHERE symbol = @symbol",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            return (long)command.ExecuteScalar();
        }

        public static long CountMinutes(string symbol, FileInfo? file = null)
        {
            using var connection = new SqliteConnection($"Data Source={Source(file ?? DbFile)}");
            connection.Open();
            using var command = new SqliteCommand(
                "SELECT COUNT(*) FROM minutes" +
                " WHERE symbol = @symbol",
                connection);
            command.Parameters.AddWithValue("@symbol", symbol);
            return (long)command.ExecuteScalar();
        }

        public static SqliteConnection CreateConnection(FileInfo file) => new($"Data Source={Source(file)}");

        private static SortedCandles ReadCandles(SqliteDataReader reader)
        {
            var builder = SortedCandles.CreateBuilder();
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

            if (!Migrated.ContainsKey(file))
            {
                using var connection = new SqliteConnection($"Data Source={file}");
                connection.Open();
                using var transaction = connection.BeginTransaction();
                Execute(
                    @"CREATE TABLE IF NOT EXISTS days(
                        symbol TEXT NOT NULL,
                        date INTEGER NOT NULL,
                        open INTEGER NOT NULL,
                        high INTEGER NOT NULL,
                        low INTEGER NOT NULL,
                        close INTEGER NOT NULL,
                        volume INTEGER NOT NULL,
                        PRIMARY KEY(symbol, date))");
                Execute(
                   @"CREATE TABLE IF NOT EXISTS minutes(
                        symbol TEXT NOT NULL,
                        time INTEGER NOT NULL,
                        open INTEGER NOT NULL,
                        high INTEGER NOT NULL,
                        low INTEGER NOT NULL,
                        close INTEGER NOT NULL,
                        volume INTEGER NOT NULL,
                        PRIMARY KEY(symbol, time))");
                Execute(
                    @"CREATE TABLE IF NOT EXISTS splits(
                        symbol TEXT NOT NULL,
                        date INTEGER NOT NULL,
                        coefficient REAL NOT NULL,
                        PRIMARY KEY(symbol, date))");
                Execute(
                    @"CREATE TABLE IF NOT EXISTS dividends(
                        symbol TEXT NOT NULL,
                        date INTEGER NOT NULL,
                        dividend REAL NOT NULL,
                        PRIMARY KEY(symbol, date))");
                Execute(
                    @"CREATE TABLE IF NOT EXISTS listings(
                        symbol TEXT NOT NULL,
                        name TEXT NULL,
                        exchange TEXT NOT NULL,
                        asset_type TEXT NOT NULL,
                        ipo_date INTEGER NOT NULL,
                        delisting_date INTEGER NULL,
                        PRIMARY KEY(symbol))");
                Execute(
                    @"CREATE TABLE IF NOT EXISTS annual_earnings(
                        symbol TEXT NOT NULL,
                        fiscal_date_ending INTEGER NOT NULL,
                        reported_eps REAL NOT NULL,
                        PRIMARY KEY(symbol, fiscal_date_ending))");
                Execute(
                    @"CREATE TABLE IF NOT EXISTS quarterly_earnings(
                        symbol TEXT NOT NULL,
                        fiscal_date_ending INTEGER NOT NULL,
                        reported_date INTEGER NOT NULL,
                        reported_eps REAL NOT NULL,
                        estimated_eps REAL NULL,
                        PRIMARY KEY(symbol, fiscal_date_ending))");
                transaction.Commit();

                Migrated.TryAdd(file, null);
                void Execute(string sql)
                {
                    using var command = connection.CreateCommand();
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                    command.CommandText = sql;
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                    command.ExecuteNonQuery();
                }
            }

            return file.FullName;
        }

        private static object ExecuteScalar(this SqliteConnection connection, string sql, SqliteParameter parameter)
        {
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
            using var command = new SqliteCommand(
                sql,
                connection);
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
            command.Parameters.Add(parameter);
            return command.ExecuteScalar();
        }

        private class FullNameComparer : IEqualityComparer<FileInfo>
        {
            internal static readonly FullNameComparer Default = new();

            private static readonly StringComparer StringComparer = StringComparer.OrdinalIgnoreCase;

            public bool Equals(FileInfo? x, FileInfo? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null)
                {
                    return false;
                }

                if (y is null)
                {
                    return false;
                }

                return StringComparer.Equals(x.FullName, y.FullName);
            }

            public int GetHashCode(FileInfo obj)
            {
                return StringComparer.GetHashCode(obj.FullName);
            }
        }
    }
}
