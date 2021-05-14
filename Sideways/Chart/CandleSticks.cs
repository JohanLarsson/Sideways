namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class CandleSticks : CandleSeries
    {
        private readonly DrawingVisual drawing;

        static CandleSticks()
        {
            RenderOptions.EdgeModeProperty.OverrideMetadata(typeof(CandleSticks), new UIPropertyMetadata(EdgeMode.Aliased));
        }

        public CandleSticks()
        {
            this.drawing = new DrawingVisual();
            this.AddVisualChild(this.drawing);
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index) => index == 0
            ? this.drawing
            : throw new ArgumentOutOfRangeException(nameof(index));

        protected override void OnRender(DrawingContext drawingContext)
        {
            var size = this.RenderSize;
            var candleWidth = this.CandleWidth;
            using var context = this.drawing.RenderOpen();
            if (this.PriceRange is { } priceRange)
            {
                if (this.CandleInterval is CandleInterval.Hour or CandleInterval.Minute)
                {
                    var x = size.Width;
                    foreach (var candle in this.Candles)
                    {
                        if (TradingDay.IsPreMarket(candle.Time))
                        {
                            context.DrawRectangle(
                                Brushes.PreMarket,
                                null,
                                Rect(
                                    new Point(x, 0),
                                    new Point(x - candleWidth, size.Height)));
                        }

                        if (TradingDay.IsPostMarket(candle.Time))
                        {
                            context.DrawRectangle(
                                Brushes.PostMarket,
                                null,
                                Rect(
                                    new Point(x, 0),
                                    new Point(x - candleWidth, size.Height)));
                        }

                        x -= candleWidth;
                        if (x < 0)
                        {
                            break;
                        }
                    }
                }

                var position = CandlePosition.Create(size.Width, candleWidth);
                foreach (var candle in this.Candles)
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
                }

                static Rect Rect(Point p1, Point p2)
                {
                    return new Rect(Round(p1), Round(p2));

                    static Point Round(Point p) => new(Math.Round(p.X), Math.Round(p.Y));
                }
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
