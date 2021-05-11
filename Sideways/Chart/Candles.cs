namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Candles
    {
        private readonly DescendingCandles days;
        private readonly DescendingCandles minutes;

        public Candles(DescendingCandles days, DescendingCandles minutes)
        {
            this.days = days;
            this.minutes = minutes;
        }

        public static Candles Adjusted(DescendingSplits splits, DescendingCandles days, DescendingCandles minutes) => new(splits.Adjust(days), splits.Adjust(minutes));

        public IEnumerable<Candle> Weeks(DateTimeOffset end)
        {
            return MergeBy(this.Days(end), (x, y) => x.Time.IsSameWeek(y.Time));
        }

        public IEnumerable<Candle> Days(DateTimeOffset end)
        {
            if (IsOpen(end) &&
                MergeBy(DayMinutes(), (_, _) => true).FirstOrNull(_ => true) is { } merged)
            {
                yield return merged;
                end = end.AddDays(-1);
            }

            foreach (var candle in this.days)
            {
                if (candle.Time <= end)
                {
                    yield return candle.WithTime(TradingDay.EndOfDay(candle.Time));
                }
            }

            IEnumerable<Candle> DayMinutes()
            {
                foreach (var minute in this.minutes)
                {
                    if (minute.Time.IsSameDay(end) &&
                        IsOpen(minute.Time) &&
                        minute.Time <= end)
                    {
                        yield return minute;
                    }
                }
            }

            bool IsOpen(DateTimeOffset t)
            {
                return t.Hour is >= 10 and <= 16 ||
                       t is { Hour: 9, Minute: >= 30 };
            }
        }

        public IEnumerable<Candle> Hours(DateTimeOffset end)
        {
            return MergeBy(this.Minutes(end), (x, y) => x.Time.IsSameHour(y.Time));
        }

        public IEnumerable<Candle> Minutes(DateTimeOffset end)
        {
            return this.minutes.Where(x => x.Time < end);
        }

        public IEnumerable<Candle> Get(DateTimeOffset end, CandleInterval interval)
        {
            return interval switch
            {
                CandleInterval.Week => this.Weeks(end),
                CandleInterval.Day => this.Days(end),
                CandleInterval.Hour => this.Hours(end),
                CandleInterval.Minute => this.Minutes(end),
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
