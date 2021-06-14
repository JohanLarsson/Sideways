namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class VWap : CandleSeries
    {
        /// <summary>Identifies the <see cref="PriceRange"/> dependency property.</summary>
        public static readonly DependencyProperty PriceRangeProperty = Chart.PriceRangeProperty.AddOwner(
            typeof(VWap),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="Brush"/> dependency property.</summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(SolidColorBrush),
            typeof(VWap),
            new FrameworkPropertyMetadata(
                default(SolidColorBrush),
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((VWap)d).pen = CreatePen((SolidColorBrush?)e.NewValue)));

        private readonly DrawingVisual drawing;

        private Pen? pen;

        public VWap()
        {
            this.drawing = new DrawingVisual();
            this.AddVisualChild(this.drawing);
        }

        public FloatRange? PriceRange
        {
            get => (FloatRange?)this.GetValue(PriceRangeProperty);
            set => this.SetValue(PriceRangeProperty, value);
        }

        public SolidColorBrush? Brush
        {
            get => (SolidColorBrush?)this.GetValue(BrushProperty);
            set => this.SetValue(BrushProperty, value);
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index) => index == 0
            ? this.drawing
            : throw new ArgumentOutOfRangeException(nameof(index));

        protected override void OnRender(DrawingContext drawingContext)
        {
            using var context = this.drawing.RenderOpen();
            if (this.pen is { } &&
                this.ItemsSource is { } candles &&
                this.PriceRange is { } priceRange)
            {
                Point? previous = null;
                var position = CandlePosition.RightToLeft(this.RenderSize, this.CandleWidth, priceRange);
                foreach (var a in candles.DescendingVWaps(this.Time, this.CandleInterval))
                {
                    var p2 = new Point(position.CenterLeft, position.Y(a));
                    if (previous is { } p1)
                    {
                        context.DrawLine(
                            this.pen,
                            p1,
                            p2);
                    }

                    previous = p2;
                    position = position.ShiftLeft();
                    if (position.Left < 0)
                    {
                        break;
                    }
                }
            }
        }

        private static Pen? CreatePen(SolidColorBrush? brush)
        {
            if (brush is { })
            {
                var temp = new Pen(brush, 1);
                temp.Freeze();
                return temp;
            }

            return null;
        }
    }
}
