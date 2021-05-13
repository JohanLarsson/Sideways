namespace Sideways
{
    using System;
    using System.Collections.Generic;

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

        public DateTimeOffset Skip(DateTimeOffset time, CandleInterval interval, int count)
        {
            return interval switch
            {
                CandleInterval.Week => TradingDay.EndOfDay(FindInterval(this.days, time, (x, y) => x.Time.IsSameWeek(y.Time), this.dayIndex, count)),
                CandleInterval.Day => TradingDay.EndOfDay(Find(this.days, time, this.dayIndex, count)),
                CandleInterval.Hour => FindInterval(this.minutes, time, (x, y) => x.Time.IsSameHour(y.Time), this.minuteIndex, count),
                CandleInterval.Minute => Find(this.minutes, time, this.minuteIndex, count),
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null),
            };

            static DateTimeOffset FindInterval(DescendingCandles candles, DateTimeOffset time, Func<Candle, Candle, bool> isSameInterval, int statAt, int count)
            {
                var index = Math.Max(0, Math.Min(candles.IndexOf(time, statAt), candles.Count - 1));
                var current = candles[index];
                var n = 0;
                if (count > 0)
                {
                    if (index > 1 &&
                        isSameInterval(candles[index], candles[index - 1]))
                    {
                        n++;
                        if (n == count)
                        {
                            return LastInInterval(index - 1);
                        }
                    }

                    for (var i = index - 1; i >= 0; i--)
                    {
                        if (!isSameInterval(current, candles[i]))
                        {
                            current = candles[i];
                            n++;
                            if (n == count)
                            {
                                return LastInInterval(i);
                            }
                        }
                    }
                }
                else
                {
                    for (var i = index + 1; i < candles.Count; i++)
                    {
                        if (!isSameInterval(current, candles[i]))
                        {
                            n--;
                            current = candles[i];
                            if (n == count)
                            {
                                return current.Time;
                            }
                        }
                    }
                }

                return LastInInterval(index);

                DateTimeOffset LastInInterval(int i)
                {
                    while (i >= 0 &&
                           isSameInterval(current, candles[i]))
                    {
                        current = candles[i];
                        i--;
                    }

                    return current.Time;
                }
            }

            static DateTimeOffset Find(DescendingCandles candles, DateTimeOffset time, int statAt, int count)
            {
                var index = Math.Max(0, Math.Min(candles.IndexOf(time, statAt) - count, candles.Count - 1));
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
