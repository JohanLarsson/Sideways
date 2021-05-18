namespace Sideways
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    public class VolumeBars : CandleSeries
    {
        private readonly DrawingVisual drawing;

        static VolumeBars()
        {
            RenderOptions.EdgeModeProperty.OverrideMetadata(typeof(VolumeBars), new UIPropertyMetadata(EdgeMode.Aliased));
        }

        public VolumeBars()
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
            if (this.Candles is { Count: > 0 } candles)
            {
                var range = new FloatRange(0, candles.Max(x => x.Volume));
                var x = size.Width;
                foreach (var candle in candles)
                {
                    var brush = Brushes.Get(candle);
                    context.DrawRectangle(
                        brush,
                        null,
                        Rect(
                            new Point(x - candleWidth + 1, size.Height),
                            new Point(x - 1, range.Y(candle.Volume, size.Height))));
                    x -= candleWidth;
                    if (x < 0)
                    {
                        break;
                    }
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
