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
            "ABBV",
            "ABCL",
            "ACRX",
            "ADSK",
            "AEP",
            "AGQ",
            "AHPI",
            "AMAT",
            "AMD",
            "AMRK",
            "ANET",
            "ANSS",
            "APPS",
            "APT",
            "ASML",
            "ATEC",
            "AYX",
            "BA",
            "BGFV",
            "BGNE",
            "BIDU",
            "BLOK",
            "BMTX",
            "BRZU",
            "BTX",
            "CAR",
            "CARA",
            "CCJ",
            "CCL",
            "CDNA",
            "CDNS",
            "CDW",
            "CELC",
            "CGEN",
            "CHIU",
            "CHNA",
            "CIBR",
            "CLLS",
            "COST",
            "COWN",
            "CPNG",
            "CRTO",
            "CRWD",
            "CSCO",
            "CSTL",
            "DAC",
            "DAR",
            "DDS",
            "DIS",
            "DISCA",
            "DISCB",
            "DOCU",
            "DOMO",
            "DSSI",
            "EA",
            "EMQQ",
            "EOG",
            "ESPO",
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
            "FTNT",
            "FUBO",
            "FUTU",
            "GALT",
            "GBTC",
            "GLD",
            "GME",
            "GOGO",
            "GOOG",
            "GSAT",
            "GTN",
            "HAL",
            "HGEN",
            "HQI",
            "HYRE",
            "HZNP",
            "IBB",
            "IBUY",
            "ICHR",
            "IDEX",
            "IGT",
            "IGV",
            "IHI",
            "IMO",
            "INO",
            "IONS",
            "IQ",
            "ISNS",
            "ITB",
            "IWM",
            "IWO",
            "IXJ",
            "IYG",
            "JD",
            "JETS",
            "JMIA",
            "JPM",
            "JYNT",
            "KBE",
            "KEX",
            "KLAC",
            "KRE",
            "LAKE",
            "LYFT",
            "M",
            "MARA",
            "MBI",
            "MCFT",
            "MEG",
            "MELI",
            "MFIN",
            "MG",
            "MJ",
            "MNK",
            "MRNA",
            "MRO",
            "MSFT",
            "MU",
            "MVIS",
            "NAKD",
            "NET",
            "NFLX",
            "NIO",
            "NM",
            "NMRK",
            "NNVC",
            "NUE",
            "NVAX",
            "NVDA",
            "NVS",
            "OCGN",
            "OLED",
            "ONCY",
            "OSTK",
            "PBW",
            "PDCO",
            "PEJ",
            "PHO",
            "PINS",
            "PKB",
            "PLL",
            "PLTR",
            "PRNT",
            "PSA",
            "PTON",
            "PULM",
            "PXLW",
            "PYPL",
            "QQQ",
            "QRVO",
            "RBLX",
            "REAL",
            "REGN",
            "REZI",
            "RIDE",
            "RIOT",
            "RLGY",
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
            "SLX",
            "SNAP",
            "SOCL",
            "SOXX",
            "SPCE",
            "SPY",
            "SQ",
            "STX",
            "TAN",
            "TBI",
            "TDOC",
            "TIGR",
            "TKAT",
            "TNA",
            "TPL",
            "TRIL",
            "TRIP",
            "TSLA",
            "TWLO",
            "U",
            "URA",
            "URG",
            "URI",
            "UROY",
            "USO",
            "UUUU",
            "W",
            "WAL",
            "WIX",
            "WOOD",
            "X",
            "XL",
            "XLB",
            "XLY",
            "XM",
            "XOP",
            "XPEV",
            "YALA",
            "YOLO",
            "Z",
            "ZEUS",
            "ZM",
        };

        private static readonly TestCaseData[] SymbolsAndSlices = Symbols.SelectMany(x => Enum.GetValues(typeof(Slice)).Cast<Slice>().Select(y => new TestCaseData(x, y))).ToArray();

        private static readonly string[] NewSymbols =
        {
            "ABCL",
            "ATEC",
            "CARA",
            "CELC",
            "COWN",
            "CPNG",
            "CRTO",
            "ICHR",
            "IWO",
            "MBI",
            "NMRK",
            "PLL",
            "REAL",
            "TRIP",
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
            var old = Database.ReadSymbols();
            foreach (var symbol in old.OrderBy(x => x))
            {
                Console.WriteLine($"            \"{symbol}\",");
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
