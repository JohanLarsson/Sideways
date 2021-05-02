namespace Sideways.Tests
{
    using System;
    using System.IO;
    using System.Net.Http;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Sideways.AlphaVantage;

    [Explicit]
    public static class Download
    {
        public static readonly string[] Symbols =
        {
            "AAPL",
            "AAT",
            "AEP",
            "ADSK",
            "AMAT",
            "AMD",
            "ANSS",
            "ASML",
            "APPS",
            "BA",
            "BIDU",
            "CCL",
            "CDW",
            "CDNS",
            "COST",
            "CSCO",
            "DIS",
            "CRWD",
            "DOCU",
            "EA",
            "EXPR",
            "F",
            "FB",
            "FOUR",
            "FSR",
            "FUBO",
            "FUTU",
            "FCEL",
            "GBTC",
            "GOGO",
            "HGEN",
            "INO",
            "JMIA",
            "JPM",
            "KLAC",
            "MARA",
            "MVIS",
            "MSFT",
            "MU",
            "NAKD",
            "NET",
            "NFLX",
            "NVAX",
            "NVDA",
            "NIO",
            "OCGN",
            "PLTR",
            "PTON",
            "PXLW",
            "PULM",
            "RBLX",
            "RIOT",
            "SAVE",
            "SHOP",
            "SNAP",
            "SPCE",
            "SQ",
            "SLQT",
            "TSLA",
            "TIGR",
            "TNA",
            "TWLO",
            "WIX",
            "X",
            "XM",
            "XPEV",
            "YALA",
            "Z",
            "ZM",
        };

        private static string ApiKey
        {
            get
            {
                var fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways/AlphaVantage.key");
                if (File.Exists(fileName))
                {
                    return File.ReadAllText(fileName).Trim();
                }

                throw new InvalidOperationException($"Expected the API key in {fileName}");
            }
        }

        [TestCaseSource(nameof(Symbols))]
        public static async Task AdjustedDays(string symbol)
        {
            using var dataSource = new DataSource(new HttpClientHandler(), ApiKey);
            await dataSource.DaysAsync(symbol).ConfigureAwait(false);
            await Task.Delay(TimeSpan.FromSeconds(12));
        }

        [TestCaseSource(nameof(Symbols))]
        public static async Task Minutes(string symbol)
        {
            if (!File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Symbols", symbol, "MINUTES.db")))
            {
                foreach (var value in Enum.GetValues(typeof(Slice)))
                {
                    using var client = new AlphaVantageClient(new HttpClientHandler(), ApiKey);
                    var candles = await client.IntervalExtendedAsync(symbol, Interval.Minute, (Slice)value);
                    Database.WriteMinutes(symbol, candles);
                    await Task.Delay(TimeSpan.FromSeconds(12));
                }
            }
        }
    }
}
