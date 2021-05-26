namespace Sideways.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;

    using NUnit.Framework;

    using Sideways.AlphaVantage;

    [Explicit]
    public static class Download
    {
        private static readonly Downloader Downloader = new();
        private static readonly AlphaVantageClient Client = new(new HttpClientHandler(), AlphaVantageClient.ApiKey, 5);

        [Test]
        public static async Task Listings()
        {
            var listings = await Client.ListingsAsync().ConfigureAwait(false);
            Database.WriteListings(listings);
        }

        [TestCaseSource(nameof(All))]
        public static async Task Days(string symbol)
        {
            var days = Database.ReadDays(symbol);
            if (days.Count == 0)
            {
                await Downloader.DaysAsync(symbol, null);
                Assert.Pass("Downloaded new symbol.");
            }

            var last = TradingDay.Create(days[0].Time);
            if (last == TradingDay.LastComplete())
            {
                Assert.Pass("Already downloaded.");
            }
            else
            {
                await Downloader.DaysAsync(symbol, last);
                Assert.Pass("Updated days for symbol.");
            }
        }

        [TestCaseSource(nameof(EmptyMinutesSource))]
        public static Task EmptyMinutes(string symbol, Slice slice)
        {
            return Minutes(symbol, slice);
        }

        [TestCaseSource(nameof(FillDownSource))]
        public static Task FillDownMinutes(string symbol, Slice slice)
        {
            return Minutes(symbol, slice);
        }

        [TestCaseSource(nameof(TopUps))]
        public static async Task TopUp(string symbol, TradingDay lastDay, DateTimeOffset lastMinute)
        {
            var candles = await Client.IntradayAsync(symbol, Interval.Minute, adjusted: false);
            if (candles.IsEmpty)
            {
                Assert.Inconclusive("Empty slice, maybe missing data on AlphaVantage. Exclude this symbol from script as it uses up daily calls.");
            }

            Database.WriteMinutes(symbol, candles);
            await Days(symbol);
            ////Assert.Fail("Check that we merge correctly first.");
            ////Assert.Fail("Adjust merged day to midnight.");
            ////var days = candles.Where(x => TradingDay.IsOrdinaryHours(x.Time)).MergeBy((x, y) => x.Time.IsSameDay(y.Time)).Select(x => x.WithTime(new DateTimeOffset(x.Time.Year, x.Time.Month, x.Time.Day, 0, 0, 0, x.Time.Offset)));
            ////Database.WriteDays(symbol, days);
        }

        private static async Task Minutes(string symbol, Slice slice)
        {
            if (slice != Slice.Year1Month1 &&
                TimeRange.FromSlice(slice) is var range &&
                Database.ReadMinutes(symbol, range.Min.AddDays(1), range.Max.AddDays(5)).Count == 0)
            {
                Assert.Pass("No slice this far back.");
            }

            var candles = await Client.IntradayExtendedAsync(symbol, Interval.Minute, slice, adjusted: false);
            if (candles.IsEmpty)
            {
                Assert.Inconclusive("Empty slice, maybe missing data on AlphaVantage. Exclude this symbol from script as it uses up daily calls.");
            }

            Database.WriteMinutes(symbol, candles);
        }

        private static IEnumerable<string> All() => Database.ReadSymbols();

        private static IEnumerable<TestCaseData> EmptyMinutesSource()
        {
            var ignore = new[]
            {
                "ACHN",
                "AMTD",
                "AMTY",
                "APPM",
                "AVX",
                "AVXS",
                "BIGG",
                "BDCO",
                "CANB",
                "CGOL",
                "CHFS",
                "CRSS",
                "CTRP",
                "CXO",
                "DERM",
                "DGAZ",
                "DMEHF",
                "DNR",
                "DO",
                "DRYS",
                "DWT",
                "DXLG",
                "EIDX",
                "EMPR",
                "ERI",
                "ETCG",
                "ETHE",
                "EWLU",
                "FBM",
                "FIT",
                "FLES",
                "FMCXF",
                "FTCO",
                "FTSV",
                "GBTC",
                "GSX",
                "HEWA",
                "HTZ",
                "I",
                "ICNB",
                "IDGX",
                "IDXG",
                "IMMU",
                "KAYS",
                "KTEL",
                "LIVC",
                "LK",
                "LM",
                "LSMG",
                "LVGO",
                "MNK",
                "NBIO",
                "NLNK",
                "OVAS",
                "PASO",
                "PNAT",
                "PUBC",
                "PTI",
                "RECAF",
                "RII",
                "SBGL",
                "SEII",
                "SICNF",
                "SYSX",
                "TBPMF",
                "TCEHY",
                "TEUM",
                "TORC",
                "TROV",
                "TPW",
                "TPTW",
                "TVIX",
                "USLV",
                "YAYO",
                "YY",
            };

            foreach (var (symbol, dayRange) in Database.DayRanges().OrderBy(x => x.Key))
            {
                if (!ignore.Contains(symbol) &&
                    Database.CountMinutes(symbol, Database.DbFile) == 0)
                {
                    foreach (var slice in Enum.GetValues<Slice>())
                    {
                        if (dayRange.Overlaps(TimeRange.FromSlice(slice)))
                        {
                            yield return new TestCaseData(symbol, slice);
                        }
                    }
                }
            }
        }

        private static IEnumerable<TestCaseData> FillDownSource()
        {
            var ignore = new[]
            {
                "ARVL",
                "BMTX",
                "BTX",
                "CARR",
                "CLOV",
                "CRNC",
                "DKNG",
                "ENVB",
                "FSR",
                "FUBO",
                "FUTU",
            };

            var minuteRanges = Database.MinuteRanges();
            foreach (var (symbol, dayRange) in Database.DayRanges().OrderBy(x => x.Key))
            {
                if (minuteRanges.TryGetValue(symbol, out var minuteRange) &&
                    dayRange.Min.Date != minuteRange.Min.Date &&
                    !ignore.Contains(symbol))
                {
                    foreach (var slice in Enum.GetValues<Slice>())
                    {
                        if (slice != Slice.Year1Month1 &&
                            dayRange.Overlaps(TimeRange.FromSlice(slice)) &&
                            !minuteRange.Contains(TimeRange.FromSlice(slice)))
                        {
                            yield return new TestCaseData(symbol, slice);
                        }
                    }
                }
            }
        }

        private static IEnumerable<TestCaseData> TopUps()
        {
            var minuteRanges = Database.MinuteRanges();
            foreach (var (symbol, dayRange) in Database.DayRanges().OrderBy(x => Last(x)))
            {
                if (TradingDay.Create(dayRange.Max) != TradingDay.LastComplete() &&
                    minuteRanges.TryGetValue(symbol, out var minuteRange) &&
                    minuteRange.Overlaps(TimeRange.FromSlice(Slice.Year1Month1)))
                {
                    yield return new(symbol, TradingDay.Create(dayRange.Max), minuteRange.Max);
                }
            }

            DateTime Last(KeyValuePair<string, TimeRange> kvp)
            {
                if (minuteRanges.TryGetValue(kvp.Key, out var minuteRange))
                {
                    return new DateTime(Math.Min(minuteRange.Max.Ticks, kvp.Value.Max.Ticks));
                }

                return DateTime.MaxValue;
            }
        }
    }
}
