namespace Sideways.Tests
{
    using System;
    using System.Collections.Generic;
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
            "ABBV",
            "ACRX",
            "ADSK",
            "AEP",
            "AGQ",
            "AHPI",
            "AMAT",
            "AMD",
            "ANSS",
            "APPS",
            "APT",
            "ASML",
            "BA",
            "BGFV",
            "BIDU",
            "BMTX",
            "BTX",
            "CCJ",
            "CCL",
            "CDNA",
            "CDNS",
            "CDW",
            "CGEN",
            "CLLS",
            "COST",
            "CRWD",
            "CSCO",
            "CSTL",
            "DAC",
            "DDS",
            "DIS",
            "DOCU",
            "DOMO",
            "DSSI",
            "EA",
            "EOG",
            "ETHE",
            "ETSY",
            "EWLU",
            "EXAS",
            "EXPR",
            "F",
            "FB",
            "FCEL",
            "FNKO",
            "FOUR",
            "FSR",
            "FUBO",
            "FUTU",
            "GALT",
            "GBTC",
            "GLD",
            "GME",
            "GOGO",
            "GOOG",
            "GTN",
            "HAL",
            "HGEN",
            "HQI",
            "HYRE",
            "HZNP",
            "IDEX",
            "IMO",
            "INO",
            "IONS",
            "IQ",
            "ISNS",
            "IWM",
            "JD",
            "JMIA",
            "JPM",
            "JYNT",
            "KEX",
            "KLAC",
            "LAKE",
            "MARA",
            "MEG",
            "MFIN",
            "MG",
            "MRNA",
            "MSFT",
            "MU",
            "MVIS",
            "NAKD",
            "NET",
            "NFLX",
            "NIO",
            "NM",
            "NNVC",
            "NUE",
            "NVAX",
            "NVDA",
            "NVS",
            "OCGN",
            "ONCY",
            "PDCO",
            "PLTR",
            "PSA",
            "PTON",
            "PULM",
            "PXLW",
            "PYPL",
            "QQQ",
            "RBLX",
            "REGN",
            "REZI",
            "RIOT",
            "ROKU",
            "RVLV",
            "SAVA",
            "SAVE",
            "SE",
            "SEAS",
            "SHIP",
            "SHOP",
            "SJR",
            "SLB",
            "SLQT",
            "SNAP",
            "SPCE",
            "SPY",
            "SQ",
            "STX",
            "TBI",
            "TDOC",
            "TIGR",
            "TKAT",
            "TNA",
            "TPL",
            "TRIL",
            "TSLA",
            "TWLO",
            "U",
            "URG",
            "URI",
            "UROY",
            "UUUU",
            "WAL",
            "WIX",
            "X",
            "XL",
            "XM",
            "XPEV",
            "YALA",
            "Z",
            "ZEUS",
            "ZM",
        };

        private static readonly TestCaseData[] SymbolsAndSlices = Symbols.SelectMany(x => Enum.GetValues(typeof(Slice)).Cast<Slice>().Select(y => new TestCaseData(x, y))).ToArray();

        private static readonly string[] NewSymbols =
        {
            "AHPI",
            "APT",
            "BGFV",
            "DOMO",
            "EOG",
            "ETSY",
            "FNKO",
            "HAL",
            "HYRE",
            "HZNP",
            "LAKE",
            "MRNA",
            "NNVC",
            "ONCY",
            "PDCO",
            "PSA",
            "REZI",
            "RVLV",
            "SAVA",
            "SEAS",
            "SLB",
            "STX",
            "TRIL",
            "URI",
        };

        private static readonly TestCaseData[] NewSymbolsAndSlices = NewSymbols.SelectMany(x => Enum.GetValues(typeof(Slice)).Cast<Slice>().Select(y => new TestCaseData(x, y))).ToArray();

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

        [Test]
        public static void Sort()
        {
            var old = new HashSet<string>(Symbols);
            foreach (var symbol in NewSymbols.OrderBy(x => x))
            {
                if (old.Add(symbol))
                {
                    Console.WriteLine($"            \"{symbol}\",");
                }
            }
        }

        [TestCaseSource(nameof(NewSymbols))]
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

        [TestCaseSource(nameof(NewSymbolsAndSlices))]
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

                Assert.Pass("No days slice this far back.");
            }

            var minutes = Database.ReadMinutes(symbol, range.Min, range.Max).Select(x => TradingDay.Create(x.Time)).Distinct().ToArray();
            if (minutes.Length == 0 &&
                slice != Slice.Year1Month1 &&
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
    }
}
