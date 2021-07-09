namespace Sideways
{
    using System;
    using System.Windows;

    public readonly struct CandlePosition : IEquatable<CandlePosition>
    {
        internal readonly double Left;
        internal readonly double Right;
        private readonly int candleWidth;
        private readonly Size renderSize;
        private readonly ValueRange valueRange;

        private CandlePosition(double left, double right, int candleWidth, Size renderSize, ValueRange valueRange)
        {
            this.Left = left;
            this.Right = right;
            this.candleWidth = candleWidth;
            this.renderSize = renderSize;
            this.valueRange = valueRange;
        }

        internal double Center => (this.Left + this.Right) / 2;

        public static bool operator ==(CandlePosition left, CandlePosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CandlePosition left, CandlePosition right)
        {
            return !left.Equals(right);
        }

        public static CandlePosition RightToLeft(Size renderSize, int candleWidth, ValueRange valueRange, int leftPad = 0, int rightPad = 0)
        {
            var right = renderSize.Width - rightPad;
            var left = right - candleWidth + rightPad + leftPad;

            return new(
                left: left,
                right: right,
                candleWidth: candleWidth,
                renderSize: renderSize,
                valueRange: valueRange);
        }

        public static CandlePosition RightToLeftPadded(Size renderSize, int candleWidth, ValueRange valueRange)
        {
            return candleWidth switch
            {
                < 2 => RightToLeft(renderSize, candleWidth, valueRange, 0, 0),
                3 => RightToLeft(renderSize, candleWidth, valueRange, 0, 1),
                4 => RightToLeft(renderSize, candleWidth, valueRange, 1, 1),
                5 => RightToLeft(renderSize, candleWidth, valueRange, 1, 1),
                < 8 => RightToLeft(renderSize, candleWidth, valueRange, 1, 1),
                < 12 => RightToLeft(renderSize, candleWidth, valueRange, 2, 2),
                < 24 => RightToLeft(renderSize, candleWidth, valueRange, 3, 3),
                _ => RightToLeft(renderSize, candleWidth, valueRange, 4, 4),
            };
        }

        public static double ClampedX(DateTimeOffset time, DescendingCandles candles, double actualWidth, int candleWidth, CandleInterval interval)
        {
            if (candles.Count == 0 ||
                time > candles[0].Time)
            {
                return actualWidth;
            }

            return X(time, candles, actualWidth, candleWidth, interval) ?? 0;
        }

        public static double? X(DateTimeOffset time, DescendingCandles candles, double actualWidth, int candleWidth, CandleInterval interval)
        {
            if (candles.Count == 0 ||
                time > candles[0].Time)
            {
                return null;
            }

            // ReSharper disable once PossibleLossOfFraction
            var x = actualWidth - (candleWidth / 2.0);
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
                    case CandleInterval.FifteenMinutes
                        when Candle.ShouldMergeFifteenMinutes(time, candle.Time):
                        return x;
                    case CandleInterval.FiveMinutes
                        when Candle.ShouldMergeFiveMinutes(time, candle.Time):
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

        public static Point? Point(TimeAndPrice timeAndPrice, DescendingCandles candles, Size renderSize, int candleWidth, CandleInterval interval, ValueRange valueRange)
        {
            if (X(timeAndPrice.Time, candles, renderSize.Width, candleWidth, interval) is { } x)
            {
                return new Point(
                    x,
                    valueRange.Y(timeAndPrice.Price, renderSize.Height));
            }

            return null;
        }

        public static double SnapCenterX(double x, int candleWidth)
        {
            return x - (x % candleWidth) + (0.5 * candleWidth);
        }

        public double Y(float value) => this.valueRange.Y(value, this.renderSize.Height);

        public TimeAndPrice? TimeAndPrice(Point position, DescendingCandles candles)
        {
            var i = (int)Math.Round((this.renderSize.Width - position.X) / this.candleWidth);
            if (i >= 0 && i < candles.Count)
            {
                return new TimeAndPrice(candles[i].Time, this.valueRange.ValueFromY(position.Y, this.renderSize.Height));
            }

            return null;
        }

        public CandlePosition ShiftLeft() => new(
            left: this.Left - this.candleWidth,
            right: this.Right - this.candleWidth,
            candleWidth: this.candleWidth,
            renderSize: this.renderSize,
            valueRange: this.valueRange);

        public bool Equals(CandlePosition other)
        {
            return this.Left.Equals(other.Left) &&
                   this.Right.Equals(other.Right) &&
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
            return HashCode.Combine(this.Left, this.Right, this.candleWidth, this.renderSize, this.valueRange);
        }
    }
}
