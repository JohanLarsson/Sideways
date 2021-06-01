namespace Sideways
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public readonly struct Candle : IEquatable<Candle>
    {
        public Candle(DateTimeOffset time, float open, float high, float low, float close, int volume)
        {
            this.Time = time;
            this.Open = open;
            this.High = high;
            this.Low = low;
            this.Close = close;
            this.Volume = volume;
        }

        public DateTimeOffset Time { get; }

        public float Open { get; }

        public float High { get; }

        public float Low { get; }

        public float Close { get; }

        public int Volume { get; }

        public static bool operator ==(Candle left, Candle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Candle left, Candle right)
        {
            return !left.Equals(right);
        }

        public static bool ShouldMergeHour(DateTimeOffset x, DateTimeOffset y)
        {
            if (x > y)
            {
                return ShouldMergeHour(y, x);
            }

            if (x.IsSameDay(y))
            {
                if (x.Hour == y.Hour && x.Minute != 0)
                {
                    if (x.Hour == 9)
                    {
                        // Start new hour candle at market open.
                        return x.Minute <= 30 == y.Minute <= 30;
                    }

                    return true;
                }

                if (x.Hour == y.Hour - 1 && x.Minute != 0 && y.Minute == 0)
                {
                    if (x.Hour == 9)
                    {
                        // Start new hour candle at market open.
                        return x.Minute > 30;
                    }

                    return true;
                }
            }

            return false;
        }

        public Candle WithTime(DateTimeOffset time) => new(
            time: time,
            open: this.Open,
            high: this.High,
            low: this.Low,
            close: this.Close,
            volume: this.Volume);

        public Candle Adjust(double coefficient) => new(
            time: this.Time,
            open: (float)coefficient * this.Open,
            high: (float)coefficient * this.High,
            low: (float)coefficient * this.Low,
            close: (float)coefficient * this.Close,
            volume: this.Volume);

        public Candle Merge(Candle other)
        {
            Debug.Assert(this.Time != other.Time, "this.Time != other.Time");
            return other.Time <= this.Time
                ? new(
                    time: this.Time,
                    open: other.Open,
                    high: Math.Max(this.High, other.High),
                    low: Math.Min(this.Low, other.Low),
                    close: this.Close,
                    volume: this.Volume + other.Volume)
                : other.Merge(this);
        }

        public bool Equals(Candle other)
        {
            return this.Time.Equals(other.Time) &&
                   this.Open.Equals(other.Open) &&
                   this.High.Equals(other.High) &&
                   this.Low.Equals(other.Low) &&
                   this.Close.Equals(other.Close) &&
                   this.Volume == other.Volume;
        }

        public override bool Equals(object? obj)
        {
            return obj is Candle other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Time, this.Open, this.High, this.Low, this.Close, this.Volume);
        }

        public override string ToString() => $"{this.Time:s} o: {this.Open} h: {this.High} l: {this.Low} c: {this.Close} volume: {this.Volume}";
    }
}
