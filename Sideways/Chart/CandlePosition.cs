namespace Sideways
{
    using System;
    using System.Windows;

    public readonly struct CandlePosition : IEquatable<CandlePosition>
    {
        internal readonly double Left;
        internal readonly double Right;
        internal readonly double CenterLeft;
        internal readonly double CenterRight;
        private readonly double candleWidth;
        private readonly Size renderSize;
        private readonly FloatRange valueRange;

        private CandlePosition(double left, double right, double centerLeft, double centerRight, double candleWidth, Size renderSize, FloatRange valueRange)
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

        public static CandlePosition Create(Size renderSize, double candleWidth, FloatRange valueRange)
        {
            var right = renderSize.Width - 1;
            var left = right - candleWidth + 2;
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
