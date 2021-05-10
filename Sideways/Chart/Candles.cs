namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Linq;

    public class Candles : IEnumerable<Candle>
    {
        private readonly ImmutableArray<Candle> candles;

        public Candles(ImmutableArray<Candle> candles)
        {
            Debug.Assert(Enumerable.SequenceEqual(candles, candles.OrderByDescending(x => x.Time)), "Candles not sorted correctly.");
            this.candles = candles;
        }

        public static Candles Adjusted(ImmutableArray<Candle> candles, ImmutableArray<Split> splits)
        {
            if (splits.IsEmpty ||
                candles.IsEmpty)
            {
                return new(candles);
            }

            var i = 0;
            var c = 1.0;
            var builder = ImmutableArray.CreateBuilder<Candle>(candles.Length);
            foreach (var candle in candles)
            {
                if (i < splits.Length &&
                    candle.Time < splits[i].Date)
                {
                    c /= splits[i].Coefficient;
                    i++;
                }

                builder.Add(candle.Adjust(c));
            }

            return new(builder.MoveToImmutable());
        }

        public IEnumerable<Candle> Get(DateTimeOffset start)
        {
            for (var i = Start(); i < this.candles.Length; i++)
            {
                var candle = this.candles[i];
                if (candle.Time > start)
                {
                    continue;
                }

                yield return candle;
            }

            int Start()
            {
                var indexOf = this.IndexOf(start);
                return indexOf > 0 ? indexOf : 0;
            }
        }

        public IEnumerable<Candle> MergeBy(DateTimeOffset start, Func<Candle, Candle, bool> criteria)
        {
            var merged = default(Candle);
            foreach (var candle in this.Get(start))
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


        public IEnumerable<Candle> Weeks(DateTimeOffset start)
        {
            return this.MergeBy(start, (x, y) => x.Time.IsSameWeek(y.Time));
        }

        public IEnumerable<Candle> Days(DateTimeOffset start)
        {
            return this.MergeBy(start, (x, y) => x.Time.IsSameDay(y.Time));
        }

        public IEnumerable<Candle> Hours(DateTimeOffset start)
        {
            return this.MergeBy(start, (x, y) => x.Time.IsSameHour(y.Time));
        }

        public IEnumerable<Candle> Get(DateTimeOffset start, CandleInterval interval)
        {
            return interval switch
            {
                CandleInterval.None => this.Get(start),
                CandleInterval.Week => this.Weeks(start),
                CandleInterval.Hour => this.Hours(start),
                _ => throw new ArgumentOutOfRangeException(nameof(interval), interval, "Unhandled grouping."),
            };
        }

        public Candle? Previous(DateTimeOffset time, CandleInterval interval)
        {
            return this.Get(time, interval).FirstOrNull(x => x.Time < time);
        }

        public Candle? Next(DateTimeOffset time, CandleInterval interval)
        {
            return this.Get(time.Add(TimeSpan.FromDays(10)), interval).LastOrNull(x => x.Time > time);
        }

        public int IndexOf(DateTimeOffset time)
        {
            if (this.candles.IsEmpty)
            {
                return -1;
            }

            var i = Clamp(new TimeRange(this.candles[^1].Time, this.candles[0].Time).Interpolate(time) * this.candles.Length);

            switch (time.CompareTo(this.candles[i].Time))
            {
                case 0:
                    return i;
                case < 0:
                    for (var j = i; j < this.candles.Length; j++)
                    {
                        switch (time.CompareTo(this.candles[j].Time))
                        {
                            case 0:
                                return j;
                            case < 0:
                                continue;
                            case > 0:
                                return -1;
                        }
                    }

                    return -1;
                case > 0:
                    for (var j = i; j >= 0; j--)
                    {
                        switch (time.CompareTo(this.candles[j].Time))
                        {
                            case 0:
                                return j;
                            case > 0:
                                continue;
                            case < 0:
                                return -1;
                        }
                    }

                    return -1;
            }

            int Clamp(double value)
            {
                if (value < 0)
                {
                    return 0;
                }

                if (value >= this.candles.Length)
                {
                    return this.candles.Length - 1;
                }

                return (int)value;
            }
        }

        public IEnumerator<Candle> GetEnumerator() => ((IEnumerable<Candle>)this.candles).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.candles).GetEnumerator();
    }
}
