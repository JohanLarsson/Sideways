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
            var days = dataSource.Days(symbol);
            if (days.Download is { } task)
            {
                await task;
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
            var days = Database.ReadDays(symbol);
            if (days.IsEmpty)
            {
                Assert.Inconclusive("Download days first");
            }

            Assert.Fail();
            //var minutes = await Database.ReadMinutes(symbol, range.Min, range.Max).ConfigureAwait(false);
            //var sliceDays = days.Where(x => range.Contains(x.Time)).ToArray();
            //CollectionAssert.IsNotEmpty(sliceDays);
            //if (sliceDays.Any(d => !minutes.Any(m => m.Time.Date == d.Time.Date)))
            //{
            //    using var client = new AlphaVantageClient(new HttpClientHandler(), ApiKey);
            //    var candles = await client.IntervalExtendedAsync(symbol, Interval.Minute, slice, adjusted: false);
            //    Database.WriteMinutes(symbol, candles);
            //}
        }
    }
}
