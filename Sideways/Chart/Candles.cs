namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Candles
    {
        private readonly SortedCandles days;
        private readonly SortedCandles minutes;

        private int dayIndex;
        private int minuteIndex;

        public Candles(SortedCandles days, SortedCandles minutes)
        {
            this.days = days;
            this.minutes = minutes;
        }

        public static Candles Adjusted(SortedSplits splits, SortedCandles days, SortedCandles minutes) => new(splits.Adjust(days), splits.Adjust(minutes));

        public IEnumerable<Candle> DescendingWeeks(DateTimeOffset end) => this.DescendingDays(end).MergeBy((x, y) => x.Time.IsSameWeek(y.Time));

        public IEnumerable<Candle> DescendingDays(DateTimeOffset end)
        {
            if (TradingDay.IsOrdinaryHours(end) &&
                this.DescendingMinutes(end).TakeWhile(x => IsSameDayOrdinaryHours(x.Time)).MergeBy((_, _) => true).FirstOrNull() is { } merged)
            {
                yield return merged;
                end = end.AddDays(-1);
            }

            if (TradingDay.IsPreMarket(end))
            {
                end = end.AddDays(-1);
            }

            var index = this.days.IndexOf(end, this.dayIndex);
            if (index < 0)
            {
                yield break;
            }

            this.dayIndex = index;
            for (var i = index; i >= 0; i--)
            {
                var day = this.days[i];
                yield return day.WithTime(TradingDay.EndOfDay(day.Time));
            }

            bool IsSameDayOrdinaryHours(DateTimeOffset time)
            {
                return time.IsSameDay(end) &&
                       TradingDay.IsOrdinaryHours(time);
            }
        }

        public IEnumerable<Candle> DescendingHours(DateTimeOffset end) => this.DescendingMinutes(end).MergeBy((x, y) => Candle.ShouldMergeHour(x.Time, y.Time));

        public IEnumerable<Candle> DescendingMinutes(DateTimeOffset end)
        {
            var index = this.minutes.IndexOf(end, this.minuteIndex);
            if (index < 0)
            {
                yield break;
            }

            this.minuteIndex = index;
            for (var i = index; i >= 0; i--)
            {
                yield return this.minutes[i];
            }
        }

        public IEnumerable<Candle> Get(DateTimeOffset end, CandleInterval interval)
        {
            return interval switch
            {
                CandleInterval.Week => this.DescendingWeeks(end),
                CandleInterval.Day => this.DescendingDays(end),
                CandleInterval.Hour => this.DescendingHours(end),
                CandleInterval.Minute => this.DescendingMinutes(end),
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unhandled grouping."),
            };
        }

        public DateTimeOffset Skip(DateTimeOffset time, CandleInterval interval, int count)
        {
            return interval switch
            {
                CandleInterval.Week => FindInterval(this.days, time, this.dayIndex, (x, y) => x.Time.IsSameWeek(y.Time), count, x => TradingDay.EndOfDay(x.Time)),
                CandleInterval.Day => Find(this.days, time, this.dayIndex, count, x => TradingDay.EndOfDay(x.Time)),
                CandleInterval.Hour => FindInterval(this.minutes, time, this.minuteIndex, (x, y) => Candle.ShouldMergeHour(x.Time, y.Time), count, x => x.Time.WithHourAndMinute(HourAndMinute.EndOfHourCandle(x.Time))),
                CandleInterval.Minute => Find(this.minutes, time, this.minuteIndex, count, x => x.Time),
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null),
            };

            static DateTimeOffset FindInterval(SortedCandles candles, DateTimeOffset time, int statAt, Func<Candle, Candle, bool> isSameInterval, int count, Func<Candle, DateTimeOffset> map)
            {
                var index = candles.IndexOf(time, statAt);
                if (index < 0)
                {
                    return time;
                }

                if (count > 0)
                {
                    for (var i = index; i < candles.Count - 1; i++)
                    {
                        if (!isSameInterval(candles[i], candles[i + 1]) &&
                            time < map(candles[i]))
                        {
                            count--;
                            if (count == 0)
                            {
                                return map(candles[i]);
                            }
                        }
                    }

                    return time <= candles[^1].Time
                        ? map(candles[^1])
                        : time;
                }

                for (var i = index; i >= 1; i--)
                {
                    if (!isSameInterval(candles[i - 1], candles[i]))
                    {
                        count++;
                        if (count == 0)
                        {
                            return map(candles[i - 1]);
                        }
                    }
                }

                return time;
            }

            static DateTimeOffset Find(SortedCandles candles, DateTimeOffset time, int statAt, int count, Func<Candle, DateTimeOffset> map)
            {
                var index = candles.IndexOf(time, statAt) + count;
                if (index < 0)
                {
                    return time;
                }

                if (index > candles.Count - 1)
                {
                    if (time <= candles[^1].Time)
                    {
                        return map(candles[^1]);
                    }

                    return time;
                }

                return map(candles[index]);
            }
        }
    }
}
