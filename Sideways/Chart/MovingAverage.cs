namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class MovingAverage : CandleSeries
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

        private readonly LayerVisual layer = new();

        private Pen? pen;

        public MovingAverage()
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

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index) => index == 0
            ? this.layer
            : throw new ArgumentOutOfRangeException(nameof(index));

        protected override void OnRender(DrawingContext drawingContext)
        {
            using var context = this.layer.RenderOpen();
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
                        context.DrawLine(this.pen, p1, p2);
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
