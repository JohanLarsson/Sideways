namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Threading.Tasks;
    using Sideways.AlphaVantage;

    public static class Csv
    {
        public static async Task<ImmutableArray<Candle>> ParseCandlesAsync(Stream content, Encoding encoding)
        {
            using var reader = new CsvReader(content, encoding);
            var header = await reader.ReadLineAsync().ConfigureAwait(false);
            if (header == "{")
            {
                throw new InvalidOperationException(await reader.ReadLineAsync().ConfigureAwait(false));
            }

            if (header != "timestamp,open,high,low,close,volume" &&
                header != "time,open,high,low,close,volume")
            {
                throw new FormatException($"Unknown header {header}");
            }

            var builder = ImmutableArray.CreateBuilder<Candle>();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false) ?? throw new FormatException("Null line");
                var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 6)
                {
                    throw new FormatException($"Illegal CSV: {line}");
                }

                builder.Add(
                    new Candle(
                        time: ReadDate(parts[0]),
                        open: float.Parse(parts[1], CultureInfo.InvariantCulture),
                        high: float.Parse(parts[2], CultureInfo.InvariantCulture),
                        low: float.Parse(parts[3], CultureInfo.InvariantCulture),
                        close: float.Parse(parts[4], CultureInfo.InvariantCulture),
                        volume: int.Parse(parts[5], CultureInfo.InvariantCulture)));
            }

            return builder.ToImmutable();

            static DateTimeOffset ReadDate(string text)
            {
                return text switch
                {
                    { Length: 10 } => DateTimeOffset.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                    { Length: 19 } => DateTimeOffset.ParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                    _ => throw new FormatException($"Unknown date format {text}"),
                };
            }
        }

        public static async Task<ImmutableArray<AdjustedCandle>> ParseAdjustedCandlesAsync(Stream content, Encoding encoding)
        {
            using var reader = new CsvReader(content, encoding);
            var header = await reader.ReadLineAsync().ConfigureAwait(false);
            if (header == "{")
            {
                throw new InvalidOperationException(await reader.ReadLineAsync().ConfigureAwait(false));
            }

            if (header != "timestamp,open,high,low,close,adjusted_close,volume,dividend_amount,split_coefficient")
            {
                throw new FormatException($"Unknown header {header}");
            }

            var builder = ImmutableArray.CreateBuilder<AdjustedCandle>();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync().ConfigureAwait(false) ?? throw new FormatException("Null line");
                var parts = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 9)
                {
                    throw new FormatException($"Illegal CSV: {line}");
                }

                builder.Add(
                    new AdjustedCandle(
                        time: ReadDate(parts[0]),
                        open: float.Parse(parts[1], CultureInfo.InvariantCulture),
                        high: float.Parse(parts[2], CultureInfo.InvariantCulture),
                        low: float.Parse(parts[3], CultureInfo.InvariantCulture),
                        close: float.Parse(parts[4], CultureInfo.InvariantCulture),
                        adjustedClose: float.Parse(parts[5], CultureInfo.InvariantCulture),
                        volume: int.Parse(parts[6], CultureInfo.InvariantCulture),
                        dividend: float.Parse(parts[7], CultureInfo.InvariantCulture),
                        splitCoefficient: float.Parse(parts[8], CultureInfo.InvariantCulture)));
            }

            return builder.ToImmutable();

            static DateTimeOffset ReadDate(string text)
            {
                return text switch
                {
                    { Length: 10 } => DateTimeOffset.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                    { Length: 19 } => DateTimeOffset.ParseExact(text, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                    _ => throw new FormatException($"Unknown date format {text}"),
                };
            }
        }
    }
}
