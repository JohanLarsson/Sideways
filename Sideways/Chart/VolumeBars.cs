namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class VolumeBars : CandleSeries
    {
        /// <summary>Identifies the <see cref="MaxVolume"/> dependency property.</summary>
        public static readonly DependencyProperty MaxVolumeProperty = Chart.MaxVolumeProperty.AddOwner(
            typeof(VolumeBars),
            new FrameworkPropertyMetadata(
                default(int?),
                FrameworkPropertyMetadataOptions.AffectsRender));

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

        public int? MaxVolume
        {
            get => (int?)this.GetValue(MaxVolumeProperty);
            protected set => this.SetValue(MaxVolumeProperty, value);
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index) => index == 0
            ? this.drawing
            : throw new ArgumentOutOfRangeException(nameof(index));

        protected override void OnRender(DrawingContext drawingContext)
        {
            var candleWidth = this.CandleWidth;
            using var context = this.drawing.RenderOpen();
            if (this.Candles is { Count: > 0 } candles &&
                this.MaxVolume is { } maxVolume)
            {
                var position = CandlePosition.CreatePadded(this.RenderSize, candleWidth, new FloatRange(0, maxVolume));
                foreach (var candle in candles)
                {
                    var brush = Brushes.Get(candle);
                    context.DrawRectangle(
                        brush,
                        null,
                        Rect(
                            new Point(position.Left, position.Y(0)),
                            new Point(position.Right, position.Y(candle.Volume))));

                    position = position.ShiftLeft();
                    if (position.Left < 0)
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
