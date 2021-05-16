namespace Sideways.Tests
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Sideways.AlphaVantage;

    [Explicit]
    public static class Download
    {
        public static readonly string[] Symbols =
        {
            "AA",
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
            "CDNA",
            "CDNS",
            "COST",
            "CSCO",
            "DIS",
            "CRWD",
            "DOCU",
            "EA",
            "EXPR",
            "ETHE",
            "F",
            "FB",
            "FOUR",
            "FSR",
            "FUBO",
            "FUTU",
            "FCEL",
            "GBTC",
            "GLD",
            "GOGO",
            "HGEN",
            "INO",
            "IONS",
            "IWM",
            "JD",
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
            "PYPL",
            "PULM",
            "QQQ",
            "RBLX",
            "REGN",
            "RIOT",
            "ROKU",
            "SAVE",
            "SE",
            "SHOP",
            "SNAP",
            "SPCE",
            "SPY",
            "SQ",
            "SLQT",
            "TDOC",
            "TSLA",
            "TIGR",
            "TNA",
            "TWLO",
            "U",
            "URG",
            "WIX",
            "X",
            "XM",
            "XPEV",
            "YALA",
            "Z",
            "ZM",
        };

        private static readonly TestCaseData[] SymbolsAndSlices = Symbols.SelectMany(x => Enum.GetValues(typeof(Slice)).Cast<Slice>().Select(y => new TestCaseData(x, y))).ToArray();
        private static readonly Downloader Downloader = new(new HttpClientHandler(), ApiKey);

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
        public static async Task Days(string symbol)
        {
            var dataSource = new DataSource(Downloader);
            if (dataSource.Days(symbol).Download is { } task)
            {
                await task;
                //// Adding an extra delay as AlphaVantage is not always happy with our throttling.
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            else
            {
                Assert.Pass("Already downloaded.");
            }
        }

        [TestCaseSource(nameof(SymbolsAndSlices))]
        public static async Task Minutes(string symbol, Slice slice)
        {
            var range = TimeRange.FromSlice(slice);
            var days = Database.ReadDays(symbol, range.Min, range.Max).Select(x => TradingDay.Create(x.Time)).Distinct().ToArray();
            if (days.Length == 0)
            {
                if (Database.ReadDays(symbol).Count == 0)
                {
                    Assert.Inconclusive("Download days first");
                }

                Assert.Pass("No slice this far back.");
            }

            var minutes = Database.ReadMinutes(symbol, range.Min, range.Max).Select(x => TradingDay.Create(x.Time)).Distinct().ToArray();
            if (minutes.Length == 0 &&
                Database.ReadMinutes(symbol, range.Min.AddDays(1), range.Max.AddDays(10)).Count == 0)
            {
                Assert.Pass("No slice this far back.");
            }

            if (!days.SequenceEqual(minutes))
            {
                using var client = new AlphaVantageClient(new HttpClientHandler(), ApiKey);
                var candles = await client.IntervalExtendedAsync(symbol, Interval.Minute, slice, adjusted: false);
                if (candles.IsEmpty)
                {
                    Assert.Inconclusive("Empty slice, maybe missing data on AlphaVantage. Exclude this symbol from script as it uses up daily calls.");
                }

                Database.WriteMinutes(symbol, candles);
                //// Adding an extra delay as AlphaVantage is not always happy with our throttling.
                await Task.Delay(TimeSpan.FromSeconds(1));
            }
            else
            {
                Assert.Pass("Already downloaded.");
            }
        }

        [TestCaseSource(nameof(Symbols))]
        public static void Copy(string symbol)
        {
            Database.Copy(symbol, Database.DbFile, new("D:\\Database.sqlite3"));
        }
    }
}
