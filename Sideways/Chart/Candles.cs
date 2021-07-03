namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
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

        public IEnumerable<Candle> DescendingFifteenMinutes(DateTimeOffset end) => this.DescendingMinutes(end).MergeBy((x, y) => Candle.ShouldMergeFifteenMinutes(x.Time, y.Time));

        public IEnumerable<Candle> DescendingFiveMinutes(DateTimeOffset end) => this.DescendingMinutes(end).MergeBy((x, y) => Candle.ShouldMergeFiveMinutes(x.Time, y.Time));

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

        public IEnumerable<float> DescendingVWaps(DateTimeOffset end, CandleInterval interval)
        {
            var ascending = new List<float>();
            foreach (var (firstMinuteIndex, lastMinuteIndex) in Indices())
            {
                ascending.Clear();
                var dollarVolume = 0f;
                var volume = 0;
                for (var i = firstMinuteIndex; i <= lastMinuteIndex; i++)
                {
                    var minute = this.minutes[i];
                    dollarVolume += Estimate(minute);
                    volume += minute.Volume;

                    if (i == lastMinuteIndex ||
                        !IsSameInterval(minute, this.minutes[i + 1]))
                    {
                        ascending.Add(dollarVolume / volume);
                    }

                    static float Estimate(Candle minute) => minute.Volume * 0.25f * (minute.Open + minute.High + minute.Low + minute.Close);

                    bool IsSameInterval(Candle x, Candle y)
                    {
                        return interval switch
                        {
                            CandleInterval.None => throw new InvalidEnumArgumentException(nameof(interval), (int)interval, typeof(CandleInterval)),
                            CandleInterval.Week => throw new NotSupportedException("Not supporting VWAP for weeks yet."),
                            CandleInterval.Day => throw new NotSupportedException("Not supporting VWAP for weeks yet."),
                            CandleInterval.Hour => Candle.ShouldMergeHour(x.Time, y.Time),
                            CandleInterval.FifteenMinutes => Candle.ShouldMergeFifteenMinutes(x.Time, y.Time),
                            CandleInterval.FiveMinutes => Candle.ShouldMergeFiveMinutes(x.Time, y.Time),
                            CandleInterval.Minute => false,
                            _ => throw new InvalidEnumArgumentException(nameof(interval), (int)interval, typeof(CandleInterval)),
                        };
                    }
                }

                for (var i = ascending.Count - 1; i >= 0; i--)
                {
                    yield return ascending[i];
                }
            }

            IEnumerable<(int, int)> Indices()
            {
                var start = this.minutes.IndexOf(end, this.minuteIndex);
                if (start < 0)
                {
                    yield break;
                }

                for (var i = start; i >= 1; i--)
                {
                    if (!this.minutes[i].Time.IsSameDay(this.minutes[i - 1].Time))
                    {
                        yield return (i, start);
                        start = i;
                    }
                }

                yield return (0, start);
            }
        }

        public IEnumerable<Candle> Get(DateTimeOffset end, CandleInterval interval)
        {
            return interval switch
            {
                CandleInterval.Week => this.DescendingWeeks(end),
                CandleInterval.Day => this.DescendingDays(end),
                CandleInterval.Hour => this.DescendingHours(end),
                CandleInterval.FifteenMinutes => this.DescendingFifteenMinutes(end),
                CandleInterval.FiveMinutes => this.DescendingFiveMinutes(end),
                CandleInterval.Minute => this.DescendingMinutes(end),
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unhandled interval."),
            };
        }

        public DateTimeOffset Skip(DateTimeOffset time, CandleInterval interval, int count)
        {
            return interval switch
            {
                CandleInterval.Week => FindInterval(this.days, time, this.dayIndex, (x, y) => x.Time.IsSameWeek(y.Time), count, x => TradingDay.EndOfDay(x.Time)),
                CandleInterval.Day => Find(this.days, time, this.dayIndex, count, x => TradingDay.EndOfDay(x.Time)),
                CandleInterval.Hour => FindInterval(this.minutes, time, this.minuteIndex, (x, y) => Candle.ShouldMergeHour(x.Time, y.Time), count, x => x.Time.WithHourAndMinute(HourAndMinute.EndOfHourCandle(x.Time))),
                CandleInterval.FifteenMinutes => FindInterval(this.minutes, time, this.minuteIndex, (x, y) => Candle.ShouldMergeFifteenMinutes(x.Time, y.Time), count, x => x.Time.WithHourAndMinute(HourAndMinute.EndOfFifteenMinutesCandle(x.Time))),
                CandleInterval.FiveMinutes => FindInterval(this.minutes, time, this.minuteIndex, (x, y) => Candle.ShouldMergeFiveMinutes(x.Time, y.Time), count, x => x.Time.WithHourAndMinute(HourAndMinute.EndOfFiveMinutesCandle(x.Time))),
                CandleInterval.Minute => Find(this.minutes, time, this.minuteIndex, count, x => x.Time),
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, null),
            };

            static DateTimeOffset FindInterval(SortedCandles candles, DateTimeOffset time, int statAt, Func<Candle, Candle, bool> isSameInterval, int count, Func<Candle, DateTimeOffset> map)
            {
                if (candles.Count == 0)
                {
                    return time;
                }

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
                if (candles.Count == 0)
                {
                    return time;
                }

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

        public Percent? Adr(DateTimeOffset time)
        {
            if (this.days.IndexOf(time, this.dayIndex) is >= 0 and var index &&
                this.days.CanSlice(index, -20))
            {
                return this.days.Slice(index, -20).Adr();
            }

            return null;
        }

        public float? Atr(DateTimeOffset time)
        {
            if (this.days.IndexOf(time, this.dayIndex) is >= 0 and var index &&
                this.days.CanSlice(index, -21))
            {
                return this.days.Slice(index, -21).Atr();
            }

            return null;
        }
    }
}
