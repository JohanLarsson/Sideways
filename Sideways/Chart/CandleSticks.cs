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

        /// <summary>Identifies the <see cref="PriceScale"/> dependency property.</summary>
        public static readonly DependencyProperty PriceScaleProperty = Chart.PriceScaleProperty.AddOwner(
            typeof(CandleSticks),
            new FrameworkPropertyMetadata(
                Scale.Logarithmic,
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

        public Scale PriceScale
        {
            get => (Scale)this.GetValue(PriceScaleProperty);
            set => this.SetValue(PriceScaleProperty, value);
        }

        protected override int VisualChildrenCount => 1;

        protected override Size MeasureOverride(Size availableSize)
        {
            this.Candles.VisibleCount = (int)Math.Ceiling(availableSize.Width / this.CandleWidth);
            return base.MeasureOverride(availableSize);
        }

        protected override Visual GetVisualChild(int index) => index == 0
            ? this.drawing
            : throw new ArgumentOutOfRangeException(nameof(index));

        protected override void OnRender(DrawingContext drawingContext)
        {
            using var context = this.drawing.RenderOpen();
            if (this.PriceRange is { } range)
            {
                var position = CandlePosition.RightToLeftPadded(this.RenderSize, this.CandleWidth, new ValueRange(range, this.PriceScale));
                foreach (var candle in this.Candles)
                {
                    var brush = Brushes.Get(candle);
                    context.DrawRectangle(
                        brush,
                        null,
                        Rect(
                            new Point(position.CenterLeft, position.Y(candle.Low)),
                            new Point(position.CenterRight, position.Y(candle.High))));
                    var yOpen = (int)position.Y(candle.Open);
                    var yClose = (int)position.Y(candle.Close);
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
