namespace Sideways
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;

    [ContentProperty(nameof(Child))]
    public class MeasureDecorator : CandleSeries
    {
        /// <summary>Identifies the <see cref="Background"/> dependency property.</summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background),
            typeof(SolidColorBrush),
            typeof(MeasureDecorator),
            new FrameworkPropertyMetadata(
                Brushes.MeasureBackground,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="PriceRange"/> dependency property.</summary>
        public static readonly DependencyProperty PriceRangeProperty = Chart.PriceRangeProperty.AddOwner(
            typeof(MeasureDecorator),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="PriceScale"/> dependency property.</summary>
        public static readonly DependencyProperty PriceScaleProperty = Chart.PriceScaleProperty.AddOwner(
            typeof(MeasureDecorator),
            new FrameworkPropertyMetadata(
                Scale.Logarithmic,
                FrameworkPropertyMetadataOptions.AffectsRender));

        private static readonly DependencyPropertyKey CurrentPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Current),
            typeof(Measurement),
            typeof(MeasureDecorator),
            new FrameworkPropertyMetadata(
                default(Measurement),
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="Current"/> dependency property.</summary>
        public static readonly DependencyProperty CurrentProperty = CurrentPropertyKey.DependencyProperty;

        private CandleSticks? child;

        static MeasureDecorator()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(MeasureDecorator), new PropertyMetadata(true));
        }

#pragma warning disable WPF0012 // CLR property type should match registered type.
        public SolidColorBrush? Background
#pragma warning restore WPF0012 // CLR property type should match registered type.
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

        [DefaultValue(null)]
        public virtual CandleSticks? Child
        {
            get => this.child;
            set
            {
                if (this.child != value)
                {
                    this.RemoveVisualChild(this.child);
                    this.RemoveLogicalChild(this.child);
                    this.child = value;
                    this.AddLogicalChild(value);
                    this.AddVisualChild(value);
                    this.InvalidateMeasure();
                }
            }
        }

        protected override IEnumerator LogicalChildren => this.child switch
        {
            { } child => new SingleChildEnumerator(child),
            _ => EmptyEnumerator.Instance,
        };

        protected override int VisualChildrenCount => this.child is null ? 0 : 1;

        protected override Visual GetVisualChild(int index)
        {
            if ((this.child is null) || (index != 0))
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Check VisualChildrenCount first");
            }

            return this.child;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.child is { } child)
            {
                child.Measure(availableSize);
                return child.DesiredSize;
            }

            return default;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.child is { } child)
            {
                child.Arrange(new Rect(finalSize));
            }

            return finalSize;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderSize = this.RenderSize;
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
    }
}
