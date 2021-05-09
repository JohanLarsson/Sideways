namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    public class SortedCandles : IEnumerable<Candle>
    {
        private readonly ImmutableArray<Candle> candles;
        private readonly ImmutableArray<Split> splits;

        public SortedCandles(ImmutableArray<Candle> candles, ImmutableArray<Split> splits)
        {
            Debug.Assert(Enumerable.SequenceEqual(candles, candles.OrderByDescending(x => x.Time)), "Candles not sorted correctly.");
            Debug.Assert(Enumerable.SequenceEqual(splits, splits.OrderByDescending(x => x.Date)), "Splits not sorted correctly.");
            this.candles = candles;
            this.splits = splits;
        }

        public IEnumerable<Candle> Get(DateTimeOffset start)
        {
            return this.candles.SkipWhile(x => x.Time > start);
        }

        public IEnumerable<Candle> GetSplitAdjusted(DateTimeOffset start)
        {
            throw new NotImplementedException();
            //var splitCoefficient = 1f;
            //foreach (var day in candles)
            //{
            //    if (day.SplitCoefficient != 0)
            //    {
            //        splitCoefficient *= day.SplitCoefficient;
            //    }

            //    yield return day.AsCandle(splitCoefficient);
            //}
        }

        public IEnumerable<Candle> Weeks(DateTimeOffset start)
        {
            var week = new List<Candle>();
            foreach (var day in this.Get(start))
            {
                if (week.Count == 0 ||
                    Week(week[0]) == Week(day))
                {
                    week.Add(day);
                }
                else
                {
                    if (week.Count > 0)
                    {
                        yield return Candle.Create(week);
                    }

                    week.Clear();
                    week.Add(day);
                }

                static int Week(Candle c) => CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(c.Time.Date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            }

            if (week.Count > 0)
            {
                yield return Candle.Create(week);
            }
        }

        IEnumerable<Candle> Hours(DateTimeOffset start)
        {
            var hour = new List<Candle>();
            foreach (var minute in this.Get(start))
            {
                if (hour.Count == 0 ||
                    Hour(hour[0]) == Hour(minute))
                {
                    hour.Add(minute);
                }
                else
                {
                    if (hour.Count > 0)
                    {
                        yield return Candle.Create(hour);
                    }

                    hour.Clear();
                    hour.Add(minute);
                }

                static int Hour(Candle c) => c.Time.DayOfYear + c.Time.Hour;
            }

            if (hour.Count > 0)
            {
                yield return Candle.Create(hour);
            }
        }

        public Candle? Previous(DateTimeOffset time)
        {
            foreach (var candle in this.candles)
            {
                if (candle.Time < time)
                {
                    return candle;
                }
            }

            return null;
        }

        public Candle? Next(DateTimeOffset time)
        {
            for (var i = this.candles.Length - 1; i >= 0; i--)
            {
                var candle = this.candles[i];
                if (candle.Time > time)
                {
                    return candle;
                }
            }

            return null;
        }

        public IEnumerator<Candle> GetEnumerator() => ((IEnumerable<Candle>)this.candles).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.candles).GetEnumerator();
    }
}
