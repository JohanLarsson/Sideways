namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class CandleSticks : CandleSeries
    {
        private readonly DrawingVisual drawing;

        /// <summary>Identifies the <see cref="PriceRange"/> dependency property.</summary>
        public static readonly DependencyProperty PriceRangeProperty = Chart.PriceRangeProperty.AddOwner(
            typeof(CandleSticks),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        static CandleSticks()
        {
            RenderOptions.EdgeModeProperty.OverrideMetadata(typeof(CandleSticks), new UIPropertyMetadata(EdgeMode.Aliased));
        }

        public CandleSticks()
        {
            this.drawing = new DrawingVisual();
            this.AddVisualChild(this.drawing);
        }

        public FloatRange? PriceRange
        {
            get => (FloatRange?)this.GetValue(PriceRangeProperty);
            set => this.SetValue(PriceRangeProperty, value);
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
            if (this.PriceRange is { } range)
            {
                var candles = this.Candles;
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

                    double Y(float price) => range.Y(price, size.Height);
                }

                static Rect Rect(Point p1, Point p2)
                {
                    return new Rect(Round(p1), Round(p2));

                    static Point Round(Point p) => new(Math.Round(p.X), Math.Round(p.Y));
                }
            }
        }
    }
}
