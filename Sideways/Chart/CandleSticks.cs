namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class CandleSticks : CandlesElement
    {
        public static readonly DependencyProperty PriceRangeProperty = Chart.PriceRangeProperty.AddOwner(
            typeof(CandleSticks),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PriceScaleProperty = Chart.PriceScaleProperty.AddOwner(
            typeof(CandleSticks),
            new FrameworkPropertyMetadata(
                Scale.Logarithmic,
                FrameworkPropertyMetadataOptions.AffectsRender));

        private static readonly Pen IncreasingPen = Brushes.CreatePen(Brushes.Increasing);
        private static readonly Pen DecreasingPen = Brushes.CreatePen(Brushes.Decreasing);

        private readonly LayerVisual layer = new();

        static CandleSticks()
        {
            RenderOptions.EdgeModeProperty.OverrideMetadata(typeof(CandleSticks), new UIPropertyMetadata(EdgeMode.Aliased));
        }

        public CandleSticks()
        {
            this.AddVisualChild(this.layer);
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

        protected override Visual GetVisualChild(int index) => index == 0
            ? this.layer
            : throw new ArgumentOutOfRangeException(nameof(index));

        protected override void OnRender(DrawingContext drawingContext)
        {
            using var context = this.layer.RenderOpen();
            if (this.PriceRange is { } range)
            {
                var position = CandlePosition.RightToLeftPadded(this.RenderSize, this.CandleWidth, new ValueRange(range, this.PriceScale));
                foreach (var candle in this.Candles)
                {
                    if (candle is { Volume: > 0, Low: > 0 })
                    {
                        context.DrawLine(
                            GetPen(candle),
                            new Point(position.Center, position.Y(candle.Low)),
                            new Point(position.Center, position.Y(candle.High)));
                        var yOpen = (int)position.Y(candle.Open);
                        var yClose = (int)position.Y(candle.Close);
                        if (yOpen == yClose)
                        {
                            yClose += 1;
                        }

                        context.DrawRectangle(
                            Brushes.Get(candle),
                            null,
                            Rect(
                                new Point(position.Left, yOpen),
                                new Point(position.Right, yClose)));
                    }

                    position = position.ShiftLeft();
                    if (position.Left < 0)
                    {
                        break;
                    }
                }

                static Pen GetPen(Candle candle) => candle.Open < candle.Close
                    ? IncreasingPen
                    : DecreasingPen;

                static Rect Rect(Point p1, Point p2)
                {
                    return new Rect(Round(p1), Round(p2));

                    static Point Round(Point p) => new(Math.Round(p.X), Math.Round(p.Y));
                }
            }
        }

        protected override void OnCandlesChanged(DescendingCandles newValue)
        {
            newValue.With(this);
            base.OnCandlesChanged(newValue);
        }
    }
}
