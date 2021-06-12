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

        public static bool operator ==(Candle left, Candle right) => left.Equals(right);

        public static bool operator !=(Candle left, Candle right) => !left.Equals(right);

        public static bool ShouldMergeHour(DateTimeOffset x, DateTimeOffset y) =>
            x.IsSameDay(y) &&
            HourAndMinute.EndOfHourCandle(x) == HourAndMinute.EndOfHourCandle(y);

        public static bool ShouldMergeFifteenMinutes(DateTimeOffset x, DateTimeOffset y) =>
            x.IsSameDay(y) &&
            HourAndMinute.EndOfFifteenMinutesCandle(x) == HourAndMinute.EndOfFifteenMinutesCandle(y);

        public static bool ShouldMergeFiveMinutes(DateTimeOffset x, DateTimeOffset y) =>
            x.IsSameDay(y) &&
            HourAndMinute.EndOfFiveMinutesCandle(x) == HourAndMinute.EndOfFiveMinutesCandle(y);

        public Candle WithTime(HourAndMinute hourAndMinute) => new(
            time: this.Time.WithHourAndMinute(hourAndMinute),
            open: this.Open,
            high: this.High,
            low: this.Low,
            close: this.Close,
            volume: this.Volume);

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
