namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Windows;

    public readonly struct CandlePosition : IEquatable<CandlePosition>
    {
        internal readonly double Left;
        internal readonly double Right;
        internal readonly double CenterLeft;
        internal readonly double CenterRight;
        private readonly int candleWidth;
        private readonly Size renderSize;
        private readonly FloatRange valueRange;

        private CandlePosition(double left, double right, double centerLeft, double centerRight, int candleWidth, Size renderSize, FloatRange valueRange)
        {
            this.Left = left;
            this.Right = right;
            this.CenterLeft = centerLeft;
            this.CenterRight = centerRight;
            this.candleWidth = candleWidth;
            this.renderSize = renderSize;
            this.valueRange = valueRange;
        }

        public static bool operator ==(CandlePosition left, CandlePosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CandlePosition left, CandlePosition right)
        {
            return !left.Equals(right);
        }

        public static CandlePosition Create(Size renderSize, int candleWidth, FloatRange valueRange, int leftPad = 0, int rightPad = 0)
        {
            var right = renderSize.Width - rightPad;
            var left = right - candleWidth + rightPad + leftPad;
            var centerRight = Math.Ceiling((right + left) / 2);

            return new(
                left: left,
                right: right,
                centerLeft: centerRight - 1,
                centerRight: centerRight,
                candleWidth: candleWidth,
                renderSize: renderSize,
                valueRange: valueRange);
        }

        public static CandlePosition CreatePadded(Size renderSize, int candleWidth, FloatRange valueRange)
        {
            return candleWidth switch
            {
                < 2 => Create(renderSize, candleWidth, valueRange, 0, 0),
                3 => Create(renderSize, candleWidth, valueRange, 0, 1),
                4 => Create(renderSize, candleWidth, valueRange, 1, 1),
                5 => Create(renderSize, candleWidth, valueRange, 1, 1),
                < 8 => Create(renderSize, candleWidth, valueRange, 1, 1),
                < 12 => Create(renderSize, candleWidth, valueRange, 2, 2),
                < 24 => Create(renderSize, candleWidth, valueRange, 3, 3),
                _ => Create(renderSize, candleWidth, valueRange, 4, 4),
            };
        }

        public static double? X(DateTimeOffset time, IReadOnlyList<Candle> candles, double actualWidth, int candleWidth, CandleInterval interval)
        {
            if (candles.Count == 0 ||
                time > candles[0].Time)
            {
                return null;
            }

            // ReSharper disable once PossibleLossOfFraction
            var x = actualWidth - (candleWidth / 2);
            foreach (var candle in candles)
            {
                switch (interval)
                {
                    case CandleInterval.Week
                        when time.IsSameWeek(candle.Time):
                        return x;
                    case CandleInterval.Day
                        when time.IsSameDay(candle.Time):
                        return x;
                    case CandleInterval.Hour
                        when Candle.ShouldMergeHour(time, candle.Time):
                        return x;
                    case CandleInterval.Minute
                        when candle.Time <= time:
                        return x;
                }

                x -= candleWidth;
                if (x < 0 ||
                    candle.Time < time)
                {
                    return null;
                }
            }

            return null;
        }

        public double Y(float value) => this.valueRange.Y(value, this.renderSize.Height);

        public CandlePosition ShiftLeft() => new(
            left: this.Left - this.candleWidth,
            right: this.Right - this.candleWidth,
            centerLeft: this.CenterLeft - this.candleWidth,
            centerRight: this.CenterRight - this.candleWidth,
            candleWidth: this.candleWidth,
            renderSize: this.renderSize,
            valueRange: this.valueRange);

        public bool Equals(CandlePosition other)
        {
            return this.Left.Equals(other.Left) &&
                   this.Right.Equals(other.Right) &&
                   this.CenterLeft.Equals(other.CenterLeft) &&
                   this.CenterRight.Equals(other.CenterRight) &&
                   this.candleWidth.Equals(other.candleWidth) &&
                   Size.Equals(this.renderSize, other.renderSize) &&
                   this.valueRange.Equals(other.valueRange);
        }

        public override bool Equals(object? obj)
        {
            return obj is CandlePosition other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Left, this.Right, this.CenterLeft, this.CenterRight, this.candleWidth, this.renderSize, this.valueRange);
        }
    }
}
