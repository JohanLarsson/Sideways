namespace Sideways
{
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    public class MeasureOverlay : CandleSeries
    {
        /// <summary>Identifies the <see cref="Background"/> dependency property.</summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background),
            typeof(SolidColorBrush),
            typeof(MeasureOverlay),
            new FrameworkPropertyMetadata(
                Brushes.MeasureBackground,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="PriceRange"/> dependency property.</summary>
        public static readonly DependencyProperty PriceRangeProperty = Chart.PriceRangeProperty.AddOwner(
            typeof(MeasureOverlay),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="PriceScale"/> dependency property.</summary>
        public static readonly DependencyProperty PriceScaleProperty = Chart.PriceScaleProperty.AddOwner(
            typeof(MeasureOverlay),
            new FrameworkPropertyMetadata(
                Scale.Logarithmic,
                FrameworkPropertyMetadataOptions.AffectsRender));

        private static readonly DependencyPropertyKey CurrentPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Current),
            typeof(Measurement),
            typeof(MeasureOverlay),
            new FrameworkPropertyMetadata(
                default(Measurement),
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="Current"/> dependency property.</summary>
        public static readonly DependencyProperty CurrentProperty = CurrentPropertyKey.DependencyProperty;

        static MeasureOverlay()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(MeasureOverlay), new PropertyMetadata(true));
        }

        public SolidColorBrush? Background
        {
            get => (SolidColorBrush?)this.GetValue(BackgroundProperty);
            set => this.SetValue(BackgroundProperty, value);
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

        public Measurement? Current
        {
            get => (Measurement?)this.GetValue(CurrentProperty);
            private set => this.SetValue(CurrentPropertyKey, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderSize = this.RenderSize;
            drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(renderSize));
            if (this.Background is { } background &&
                this.PriceRange is { } priceRange &&
                this.Current is { End: { } end } measurement)
            {
                var candlePosition = CandlePosition.RightToLeft(renderSize, this.CandleWidth, new ValueRange(priceRange, this.PriceScale));
                if (candlePosition.Point(measurement.Start, this.Candles, this.CandleInterval) is { } p1 &&
                    candlePosition.Point(end, this.Candles, this.CandleInterval) is { } p2)
                {
                    drawingContext.DrawRectangle(
                        background,
                        null,
                        new Rect(
                            p1,
                            p2));
                }
            }
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (this.PriceRange is { } priceRange &&
                CandlePosition.RightToLeft(this.RenderSize, this.CandleWidth, new ValueRange(priceRange, this.PriceScale)).TimeAndPrice(e.GetPosition(this), this.Candles) is { } start)
            {
                this.Current = new Measurement(start, null);
            }
            else
            {
                this.Current = null;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.PriceRange is { } priceRange &&
                e.LeftButton == MouseButtonState.Pressed &&
                CandlePosition.RightToLeft(this.RenderSize, this.CandleWidth, new ValueRange(priceRange, this.PriceScale)).TimeAndPrice(e.GetPosition(this), this.Candles) is { } timeAndPrice)
            {
                if (this.Current is { } measurement)
                {
                    this.Current = new Measurement(measurement.Start, timeAndPrice);
                }
                else
                {
                    this.Current = new Measurement(timeAndPrice, null);
                }
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.Current = null;
            base.OnMouseLeave(e);
        }

        public class Measurement
        {
            public Measurement(TimeAndPrice start, TimeAndPrice? end)
            {
                this.Start = start;
                this.End = end;
            }

            public TimeAndPrice Start { get; }

            public TimeAndPrice? End { get; }
        }
    }
}
