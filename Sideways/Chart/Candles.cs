namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    public class Candles
    {
        private readonly DescendingCandles days;
        private readonly DescendingCandles minutes;

        private int dayIndex;
        private int minuteIndex;

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
            if (TradingDay.IsOrdinaryHours(end) &&
                MergeBy(DayMinutes(), (_, _) => true).FirstOrNull(_ => true) is { } merged)
            {
                yield return merged;
                end = end.AddDays(-1);
            }

            if (TradingDay.IsPreMarket(end))
            {
                end = end.AddDays(-1);
            }

            var index = this.days.IndexOf(end, this.dayIndex);
            this.dayIndex = index;
            for (var i = index; i < this.days.Count; i++)
            {
                var day = this.days[i];
                yield return day.WithTime(TradingDay.EndOfDay(day.Time));
            }

            IEnumerable<Candle> DayMinutes()
            {
                foreach (var minute in this.Minutes(end))
                {
                    if (minute.Time.IsSameDay(end) &&
                        TradingDay.IsOrdinaryHours(minute.Time))
                    {
                        yield return minute;
                    }
                }
            }
        }

        public IEnumerable<Candle> Hours(DateTimeOffset end)
        {
            return MergeBy(this.Minutes(end), (x, y) => x.Time.IsSameHour(y.Time));
        }

        public IEnumerable<Candle> Minutes(DateTimeOffset end)
        {
            var index = this.minutes.IndexOf(end, this.minuteIndex);
            this.minuteIndex = index;
            for (var i = index; i < this.minutes.Count; i++)
            {
                yield return this.minutes[i];
            }
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

        public DateTimeOffset Skip(DateTimeOffset end, CandleInterval interval, int offset)
        {
            return interval switch
            {
                CandleInterval.Week => FindWeek(this.days, end, this.dayIndex, offset),
                CandleInterval.Day => TradingDay.EndOfDay(Find(this.days, end, this.dayIndex, offset)),
                CandleInterval.Minute => Find(this.minutes, end, this.minuteIndex, offset),
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null),
            };

            static DateTimeOffset FindWeek(DescendingCandles days, DateTimeOffset end, int statAt, int offset)
            {
                var index = Math.Max(0, Math.Min(days.IndexOf(Sunday().AddDays(-7 * offset), statAt), days.Count - 1));
                return TradingDay.EndOfDay(days[index].Time);

                DateTimeOffset Sunday()
                {
                    return end.DayOfWeek switch
                    {
                        DayOfWeek.Sunday => end,
                        DayOfWeek.Monday => end.AddDays(6),
                        DayOfWeek.Tuesday => end.AddDays(5),
                        DayOfWeek.Wednesday => end.AddDays(4),
                        DayOfWeek.Thursday => end.AddDays(3),
                        DayOfWeek.Friday => end.AddDays(2),
                        DayOfWeek.Saturday => end.AddDays(1),
                        _ => throw new InvalidEnumArgumentException(),
                    };
                }
            }

            static DateTimeOffset Find(DescendingCandles candles, DateTimeOffset end, int statAt, int offset)
            {
                var index = Math.Max(0, Math.Min(candles.IndexOf(end, statAt) + offset, candles.Count - 1));
                return candles[index].Time;
            }
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
