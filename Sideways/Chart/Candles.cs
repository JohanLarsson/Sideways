namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Candles
    {
        private readonly DescendingDays days;
        private readonly DescendingMinutes minutes;

        public Candles(DescendingDays days, DescendingMinutes minutes)
        {
            this.days = days;
            this.minutes = minutes;
        }

        public static Candles Adjusted(DescendingSplits splits, DescendingDays days, DescendingMinutes minutes) => new(splits.Adjust(days), splits.Adjust(minutes));

        public IEnumerable<Candle> Weeks(DateTimeOffset end)
        {
            return MergeBy(this.Days(end), (x, y) => x.Time.IsSameWeek(y.Time));
        }

        public IEnumerable<Candle> Days(DateTimeOffset end)
        {
            return this.days.Where(x => x.Time < end);
        }

        public IEnumerable<Candle> Hours(DateTimeOffset end)
        {
            return MergeBy(this.Minutes(end), (x, y) => x.Time.IsSameHour(y.Time));
        }

        public IEnumerable<Candle> Minutes(DateTimeOffset end)
        {
            return this.minutes.Where(x => x.Time < end);
        }

        public IEnumerable<Candle> Get(DateTimeOffset start, CandleInterval interval)
        {
            return interval switch
            {
                CandleInterval.Week => this.Weeks(start),
                CandleInterval.Day => this.Days(start),
                CandleInterval.Hour => this.Hours(start),
                CandleInterval.Minute => this.Minutes(start),
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unhandled grouping."),
            };
        }

        private static IEnumerable<Candle> MergeBy(IEnumerable<Candle> candles, Func<Candle, Candle, bool> criteria)
        {
            var merged = default(Candle);
            foreach (var candle in candles)
            {
                if (merged == default)
                {
                    merged = candle;
                }
                else if (criteria(merged, candle))
                {
                    merged = merged.Merge(candle);
                }
                else
                {
                    yield return merged;
                    merged = candle;
                }
            }

            if (merged != default)
            {
                yield return merged;
            }
        }
    }
}
