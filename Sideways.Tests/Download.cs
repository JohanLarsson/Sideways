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
            "ADSK",
            "AEP",
            "AMAT",
            "AMD",
            "AMRK",
            "ANSS",
            "APPS",
            "ASML",
            "BA",
            "BIDU",
            "BTX",
            "CAR",
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
            "DAR",
            "DDS",
            "DIS",
            "DISCB",
            "DOCU",
            "DSSI",
            "EA",
            "ETHE",
            "EWLU",
            "EXAS",
            "EXPR",
            "F",
            "FB",
            "FCEL",
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
            "GSAT",
            "HGEN",
            "IGT",
            "IMO",
            "INO",
            "IONS",
            "ISNS",
            "IWM",
            "JD",
            "JMIA",
            "JPM",
            "JYNT",
            "KEX",
            "KLAC",
            "LYFT",
            "MARA",
            "MCFT",
            "MEG",
            "MFIN",
            "MG",
            "MRO",
            "MSFT",
            "MU",
            "MVIS",
            "NAKD",
            "NET",
            "NFLX",
            "NIO",
            "NM",
            "NUE",
            "NVAX",
            "NVDA",
            "NVS",
            "OCGN",
            "PLTR",
            "PTON",
            "PULM",
            "PXLW",
            "PYPL",
            "QQQ",
            "RBLX",
            "REGN",
            "RIDE",
            "RIOT",
            "ROKU",
            "SAVE",
            "SE",
            "SHIP",
            "SHOP",
            "SJR",
            "SLQT",
            "SNAP",
            "SPCE",
            "SPY",
            "SQ",
            "TBI",
            "TDOC",
            "TIGR",
            "TKAT",
            "TNA",
            "TPL",
            "TSLA",
            "TWLO",
            "U",
            "URG",
            "UROY",
            "WAL",
            "WIX",
            "X",
            "XM",
            "XPEV",
            "YALA",
            "Z",
            "ZEUS",
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

        [Test]
        public static void Sort()
        {
            var set = new HashSet<string>(Symbols)
            {
                "GME",
                "MRO",
                "GSAT",
                "MCFT",
                "IGT",
                "DAR",
                "AMRK",
                "GOGO",
                "CAR",
                "RIDE",
                "LYFT",
            };
            foreach (var symbol in set.OrderBy(x => x))
            {
                Console.WriteLine($"            \"{symbol}\",");
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

                Assert.Pass("No days slice this far back.");
            }

            var minutes = Database.ReadMinutes(symbol, range.Min, range.Max).Select(x => TradingDay.Create(x.Time)).Distinct().ToArray();
            if (minutes.Length == 0 &&
                slice != Slice.Year1Month1 &&
                Database.ReadMinutes(symbol, range.Min.AddDays(1), range.Max.AddDays(5)).Count == 0)
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
