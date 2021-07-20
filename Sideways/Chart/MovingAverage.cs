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

        static MovingAverage()
        {
            IsHitTestVisibleProperty.OverrideMetadata(typeof(MovingAverage), new UIPropertyMetadata(false));
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

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.Stroke is { } stroke &&
                this.PriceRange is { } priceRange &&
                this.Candles is { } candles &&
                this.Period is > 0 and var period &&
                candles.Count > period)
            {
                this.pen ??= Brushes.CreatePen(stroke);
                Point? previous = null;
                var position = CandlePosition.RightToLeft(this.RenderSize, this.CandleWidth, new ValueRange(priceRange, this.PriceScale));

                var sum = 0f;
                var buffer = new float[period];
                var n = 0;
                var span = candles.AsSpan();
                var startAt = 0;
                for (var i = 0; i < span.Length; i++)
                {
                    var candle = span[i];
                    if (candle.Volume > 0)
                    {
                        n++;
                        var value = candle.Close;
                        sum += value;
                        buffer[n] = value;

                        if (n == period - 1)
                        {
                            startAt = i + 1;
                            break;
                        }
                    }
                }

                for (var i = startAt; i < span.Length; i++)
                {
                    var candle = span[i];
                    if (candle.Volume > 0)
                    {
                        n++;
                        var value = candle.Close;
                        sum += value;
                        var index = n % period;
                        sum -= buffer[index];
                        var p2 = new Point(position.Center, position.Y(sum / period));
                        if (previous is { } p1)
                        {
                            drawingContext.DrawLine(this.pen, p1, p2);
                        }

                        previous = p2;

                        buffer[index] = value;
                    }

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
