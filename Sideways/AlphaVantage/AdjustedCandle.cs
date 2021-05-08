namespace Sideways
{
    using System;

    [System.Diagnostics.DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct AdjustedCandle : IEquatable<AdjustedCandle>
    {
        public AdjustedCandle(DateTimeOffset time, float open, float high, float low, float close, float adjustedClose, int volume, float dividend, float splitCoefficient)
        {
            this.Time = time;
            this.Open = open;
            this.High = high;
            this.Low = low;
            this.Close = close;
            this.AdjustedClose = adjustedClose;
            this.Volume = volume;
            this.Dividend = dividend;
            this.SplitCoefficient = splitCoefficient;
        }

        public DateTimeOffset Time { get; }

        public float Open { get; }

        public float High { get; }

        public float Low { get; }

        public float Close { get; }

        public float AdjustedClose { get; }

        public int Volume { get; }

        public float Dividend { get; }

        public float SplitCoefficient { get; }

        public static bool operator ==(AdjustedCandle left, AdjustedCandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AdjustedCandle left, AdjustedCandle right)
        {
            return !left.Equals(right);
        }

        public bool Equals(AdjustedCandle other)
        {
            return this.Time.Equals(other.Time) &&
                   this.Open.Equals(other.Open) &&
                   this.High.Equals(other.High) &&
                   this.Low.Equals(other.Low) &&
                   this.Close.Equals(other.Close) &&
                   this.AdjustedClose.Equals(other.AdjustedClose) &&
                   this.Volume == other.Volume &&
                   this.Dividend.Equals(other.Dividend) &&
                   this.SplitCoefficient.Equals(other.SplitCoefficient);
        }

        public override bool Equals(object? obj)
        {
            return obj is AdjustedCandle other && this.Equals(other);
        }

        public override int GetHashCode()
        {
#pragma warning disable SA1129 // Do not use default value type constructor
            var hashCode = new HashCode();
#pragma warning restore SA1129 // Do not use default value type constructor
            hashCode.Add(this.Time);
            hashCode.Add(this.Open);
            hashCode.Add(this.High);
            hashCode.Add(this.Low);
            hashCode.Add(this.Close);
            hashCode.Add(this.AdjustedClose);
            hashCode.Add(this.Volume);
            hashCode.Add(this.Dividend);
            hashCode.Add(this.SplitCoefficient);
            return hashCode.ToHashCode();
        }

        public Candle AsCandle(float splitCoefficient)
        {
            return new(this.Time, splitCoefficient * this.Open, splitCoefficient * this.High, splitCoefficient * this.Low, splitCoefficient * this.Close, this.Volume);
        }

        private string GetDebuggerDisplay() => $"{this.Time} o: {this.Open} h: {this.High} l: {this.Low} c: {this.Close} volume: {this.Volume}";
    }
}
