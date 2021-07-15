namespace Sideways
{
    using System.Windows;
    using System.Windows.Media;

    public class MovingAverage : CandlesElement
    {
        public static readonly DependencyProperty PriceRangeProperty = Chart.PriceRangeProperty.AddOwner(
            typeof(MovingAverage),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PriceScaleProperty = Chart.PriceScaleProperty.AddOwner(
            typeof(MovingAverage),
            new FrameworkPropertyMetadata(
                Scale.Logarithmic,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            nameof(Stroke),
            typeof(SolidColorBrush),
            typeof(MovingAverage),
            new FrameworkPropertyMetadata(
                default(SolidColorBrush),
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, _) => ((MovingAverage)d).pen = null));

        public static readonly DependencyProperty PeriodProperty = DependencyProperty.Register(
            nameof(Period),
            typeof(int),
            typeof(MovingAverage),
            new PropertyMetadata(
                default(int),
                (o, e) => ((MovingAverage)o).Candles.WithExtra((int)e.NewValue)));

        private Pen? pen;

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

        public SolidColorBrush? Stroke
        {
            get => (SolidColorBrush?)this.GetValue(StrokeProperty);
            set => this.SetValue(StrokeProperty, value);
        }

        public int Period
        {
            get => (int)this.GetValue(PeriodProperty);
            set => this.SetValue(PeriodProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.Stroke is { } stroke &&
                this.PriceRange is { } priceRange)
            {
                this.pen ??= Brushes.CreatePen(stroke);
                Point? previous = null;
                var position = CandlePosition.RightToLeft(this.RenderSize, this.CandleWidth, new ValueRange(priceRange, this.PriceScale));
                foreach (var a in this.Candles.MovingAverage(this.Period, c => c.Close))
                {
                    var p2 = new Point(position.Center, position.Y(a));
                    if (previous is { } p1)
                    {
                        drawingContext.DrawLine(this.pen, p1, p2);
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
    }
}
