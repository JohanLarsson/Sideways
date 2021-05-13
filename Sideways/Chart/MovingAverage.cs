namespace Sideways
{
    using System;
    using System.Linq;
    using System.Windows;
    using System.Windows.Media;

    public class MovingAverage : CandleSeries
    {
        /// <summary>Identifies the <see cref="Brush"/> dependency property.</summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(SolidColorBrush),
            typeof(MovingAverage),
            new PropertyMetadata(
                default(SolidColorBrush),
                (d, e) => ((MovingAverage)d).pen = CreatePen((SolidColorBrush?)e.NewValue)));

        /// <summary>Identifies the <see cref="Period"/> dependency property.</summary>
        public static readonly DependencyProperty PeriodProperty = DependencyProperty.Register(
            nameof(Period),
            typeof(int),
            typeof(MovingAverage),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref="PriceRange"/> dependency property.</summary>
        public static readonly DependencyProperty PriceRangeProperty = DependencyProperty.Register(
            nameof(PriceRange),
            typeof(FloatRange?),
            typeof(MovingAverage),
            new FrameworkPropertyMetadata(
                default(FloatRange?),
                FrameworkPropertyMetadataOptions.AffectsRender));

        private readonly DrawingVisual drawing;

        private Pen? pen;

        public MovingAverage()
        {
            this.drawing = new DrawingVisual();
            this.AddVisualChild(this.drawing);
        }

        public SolidColorBrush? Brush
        {
            get => (SolidColorBrush?)this.GetValue(BrushProperty);
            set => this.SetValue(BrushProperty, value);
        }

        public int Period
        {
            get => (int)this.GetValue(PeriodProperty);
            set => this.SetValue(PeriodProperty, value);
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
            if (this.ItemsSource is { } itemsSource &&
                this.pen is { } &&
                this.PriceRange is { } priceRange)
            {
                using var context = this.drawing.RenderOpen();
                Point? previous = null;
                var x = size.Width - (candleWidth / 2);
                foreach (var a in itemsSource.Get(this.Time, this.CandleInterval)
                                                  .MovingAverage(this.Period, x => x.Close)
                                                  .Take((int)Math.Ceiling(size.Width / candleWidth)))
                {
                    var p2 = new Point(x, Y(a));
                    if (previous is { } p1)
                    {
                        context.DrawLine(
                            this.pen,
                            p1,
                            p2);
                    }

                    previous = p2;
                    x -= this.CandleWidth;
                    double Y(float price) => priceRange.Y(price, size.Height);
                }
            }
            else
            {
                // clear
                using var context = this.drawing.RenderOpen();
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
