namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    public class CandleSticks : CandleSeries
    {
        private readonly DrawingVisual drawing;

        public CandleSticks()
        {
            this.drawing = new DrawingVisual();
            this.AddVisualChild(this.drawing);
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index) => index == 0
            ? this.drawing
            : throw new ArgumentOutOfRangeException(nameof(index));

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (this.ItemsSource is { } candles)
            {
                this.SetCurrentValue(TimeProperty, candles.Skip(this.Time, this.CandleInterval, Math.Sign(e.Delta)));
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var size = this.RenderSize;
            drawingContext.DrawRectangle(
                System.Windows.Media.Brushes.Transparent,
                null,
                new Rect(this.RenderSize));

            var candleWidth = this.CandleWidth;
            if (this.ItemsSource is { } itemsSource)
            {
                var builder = ImmutableArray.CreateBuilder<Candle>((int)Math.Ceiling(size.Width / candleWidth));
                var min = float.MaxValue;
                var max = float.MinValue;
                var x = 0.0;
                foreach (var candle in itemsSource.Get(this.Time, this.CandleInterval))
                {
                    if (x > size.Width)
                    {
                        break;
                    }

                    min = Math.Min(min, candle.Low);
                    max = Math.Max(max, candle.High);
                    builder.Add(candle);
                    x += candleWidth;
                }

                var priceRange = new FloatRange(min, max);
                this.PriceRange = priceRange;
                this.VisibleCandles = builder.ToImmutable();
                using var context = this.drawing.RenderOpen();
                var position = CandlePosition.Create(size.Width, candleWidth);

                foreach (var candle in builder)
                {
                    var brush = Brushes.Get(candle);

                    context.DrawRectangle(
                        brush,
                        null,
                        Rect(
                            new Point(position.CenterLeft, Y(candle.Low)),
                            new Point(position.CenterRight, Y(candle.High))));
                    var yOpen = (int)Y(candle.Open);
                    var yClose = (int)Y(candle.Close);
                    if (yOpen == yClose)
                    {
                        yClose += 1;
                    }

                    context.DrawRectangle(
                        brush,
                        null,
                        Rect(
                            new Point(position.Left, yOpen),
                            new Point(position.Right, yClose)));

                    position = position.Shift();
                    if (position.Right < 0)
                    {
                        break;
                    }

                    double Y(float price) => priceRange.Y(price, size.Height);

                    static Rect Rect(Point p1, Point p2)
                    {
                        return new Rect(Round(p1), Round(p2));

                        static Point Round(Point p) => new(Math.Round(p.X), Math.Round(p.Y));
                    }
                }
            }
            else
            {
                this.VisibleCandles = null;
                this.PriceRange = null;

                // clear
                using var context = this.drawing.RenderOpen();
            }
        }

        private readonly struct CandlePosition
        {
            internal readonly double Left;
            internal readonly double Right;
            internal readonly double CenterLeft;
            internal readonly double CenterRight;
            private readonly double width;

            private CandlePosition(double left, double right, double centerLeft, double centerRight, double width)
            {
                this.Left = left;
                this.Right = right;
                this.CenterLeft = centerLeft;
                this.CenterRight = centerRight;
                this.width = width;
            }

            internal static CandlePosition Create(double width, double candleWidth)
            {
                var right = width - 1;
                var left = right - candleWidth + 2;
                var centerRight = Math.Ceiling((right + left) / 2);

                return new(
                    left: left,
                    right: right,
                    centerLeft: centerRight - 1,
                    centerRight: centerRight,
                    width: candleWidth);
            }

            internal CandlePosition Shift() => new(
                left: this.Left - this.width,
                right: this.Right - this.width,
                centerLeft: this.CenterLeft - this.width,
                centerRight: this.CenterRight - this.width,
                width: this.width);
        }
    }
}
