namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class DescendingCandles : IReadOnlyList<Candle>, INotifyPropertyChanged
    {
        private readonly List<Candle> candles = new();

        private int extraCount;
        private CandleSticks? candleSticks;
        private FloatRange? priceRange;
        private TimeRange? timeRange;
        private int maxVolume;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Count => this.candles.Count;

        public FloatRange? PriceRange
        {
            get => this.priceRange;
            private set
            {
                if (value == this.priceRange)
                {
                    return;
                }

                this.priceRange = value;
                this.OnPropertyChanged();
            }
        }

        public TimeRange? TimeRange
        {
            get => this.timeRange;
            private set
            {
                if (value == this.timeRange)
                {
                    return;
                }

                this.timeRange = value;
                this.OnPropertyChanged();
            }
        }

        public int MaxVolume
        {
            get => this.maxVolume;
            private set
            {
                if (value == this.maxVolume)
                {
                    return;
                }

                this.maxVolume = value;
                this.OnPropertyChanged();
            }
        }

        public Candle this[int index] => this.candles[index];

        // ReSharper disable once UnusedMember.Global
        public ReadOnlySpan<Candle> Slice(int start, int length) => CollectionsMarshal.AsSpan(this.candles).Slice(start, length);

        public IEnumerator<Candle> GetEnumerator() => this.candles.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)this.candles).GetEnumerator();

        public void Refresh(Candles? itemsSource, DateTimeOffset time, CandleInterval interval)
        {
            this.candles.Clear();
            var visibleCount = this.candleSticks is { RenderSize: { Width: > 0 and var width }, CandleWidth: > 0 and var candleWidth }
                ? (int)Math.Ceiling(width / candleWidth)
                : 0;

            var min = float.MaxValue;
            var max = float.MinValue;
            var minTime = default(DateTimeOffset);
            var vol = 0;
            if (visibleCount > 0 &&
                itemsSource is { })
            {
                foreach (var candle in itemsSource.Descending(time, interval))
                {
                    this.candles.Add(candle);
                    if (this.candles.Count <= visibleCount)
                    {
                        min = Math.Min(min, candle.Low);
                        max = Math.Max(max, candle.High);
                        vol = Math.Max(vol, candle.Volume);
                        minTime = candle.Time;
                    }

                    if (this.candles.Count >= visibleCount + this.extraCount)
                    {
                        break;
                    }
                }
            }

            if (this.candles.Count > 0)
            {
                this.PriceRange = new FloatRange(min, max);
                this.TimeRange = new TimeRange(minTime, time);
                this.MaxVolume = vol;
            }
            else
            {
                this.PriceRange = null;
                this.TimeRange = null;
                this.MaxVolume = 0;
            }
        }

        public void Add(Candle candle) => this.candles.Add(candle);

        public void WithExtra(int count)
        {
            this.extraCount = Math.Max(this.extraCount, count);
        }

        public void With(CandleSticks candleSticks)
        {
            this.candleSticks = candleSticks;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
