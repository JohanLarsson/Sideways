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
                var candles = this.Candles;
                if (this.CandleInterval is CandleInterval.Hour or CandleInterval.Minute)
                {
                    for (var i = 0; i < Math.Min(candles.Count, Math.Ceiling(size.Width / candleWidth)); i++)
                    {
                        var candle = candles[i];
                        if (TradingDay.IsPreMarket(candle.Time))
                        {
                            var p1 = new Point(X(i), 0);
                            Skip(c => TradingDay.IsPreMarket(c.Time));
                            context.DrawRectangle(
                                Brushes.PreMarket,
                                null,
                                Rect(
                                    p1,
                                    new Point(X(i + 1), size.Height)));
                        }
                        else if (TradingDay.IsPostMarket(candle.Time))
                        {
                            var p1 = new Point(X(i), 0);
                            Skip(c => TradingDay.IsPostMarket(c.Time));

                            context.DrawRectangle(
                                Brushes.PostMarket,
                                null,
                                Rect(
                                    p1,
                                    new Point(X(i + 1), size.Height)));
                        }

                        void Skip(Func<Candle, bool> selector)
                        {
                            i++;
                            while (i < candles.Count - 1 &&
                                   selector(candles[i + 1]))
                            {
                                i++;
                            }
                        }

                        double X(int index) => Math.Max(0, size.Width - (index * candleWidth));
                    }
                }

                var position = CandlePosition.Create(size.Width, candleWidth);
                foreach (var candle in candles)
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
