namespace Sideways
{
    using System;
    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;

    [ContentProperty(nameof(Children))]
    public class Chart : FrameworkElement
    {
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached(
            nameof(ItemsSource),
            typeof(Candles),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                default(Candles),
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty TimeProperty = DependencyProperty.RegisterAttached(
            nameof(Time),
            typeof(DateTimeOffset),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                default(DateTimeOffset),
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                (o, _) => (o as Chart)?.Refresh()));

        public static readonly DependencyProperty CandleIntervalProperty = DependencyProperty.RegisterAttached(
            nameof(CandleInterval),
            typeof(CandleInterval),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                CandleInterval.None,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange,
                (o, _) => (o as Chart)?.Refresh()));

        public static readonly DependencyProperty CandleWidthProperty = DependencyProperty.RegisterAttached(
            nameof(CandleWidth),
            typeof(int),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                5,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange,
                (o, _) => (o as Chart)?.Refresh()));

        public static readonly DependencyProperty PriceScaleProperty = DependencyProperty.RegisterAttached(
            nameof(PriceScale),
            typeof(Scale),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                Scale.Logarithmic,
                FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty CandlesProperty = DependencyProperty.RegisterAttached(
            nameof(Candles),
            typeof(DescendingCandles),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                default(DescendingCandles),
                FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty TimeRangeProperty = DependencyProperty.RegisterAttached(
            nameof(TimeRange),
            typeof(TimeRange?),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty PriceRangeProperty = DependencyProperty.RegisterAttached(
            nameof(PriceRange),
            typeof(FloatRange?),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.Inherits));

        public static readonly DependencyProperty MaxVolumeProperty = DependencyProperty.RegisterAttached(
            nameof(MaxVolume),
            typeof(int?),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                default(int?),
                FrameworkPropertyMetadataOptions.Inherits));

        static Chart()
        {
            UseLayoutRoundingProperty.OverrideMetadata(
                typeof(Chart),
                new FrameworkPropertyMetadata(
                    true,
                    FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsMeasure));
        }

        public Chart()
        {
            this.Children = new UIElementCollection(this, this);
            this.Candles = new DescendingCandles();
        }

        public UIElementCollection Children { get; }

        public Candles? ItemsSource
        {
            get => (Candles?)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public DateTimeOffset Time
        {
            get => (DateTimeOffset)this.GetValue(TimeProperty);
            set => this.SetValue(TimeProperty, value);
        }

        public CandleInterval CandleInterval
        {
            get => (CandleInterval)this.GetValue(CandleIntervalProperty);
            set => this.SetValue(CandleIntervalProperty, value);
        }

        public int CandleWidth
        {
            get => (int)this.GetValue(CandleWidthProperty);
            set => this.SetValue(CandleWidthProperty, value);
        }

        public Scale PriceScale
        {
            get => (Scale)this.GetValue(PriceScaleProperty);
            set => this.SetValue(PriceScaleProperty, value);
        }

#pragma warning disable WPF0012 // CLR property type should match registered type.
        public DescendingCandles Candles
#pragma warning restore WPF0012 // CLR property type should match registered type.
        {
            get => (DescendingCandles)this.GetValue(CandlesProperty);
            set => this.SetValue(CandlesProperty, value);
        }

        public TimeRange? TimeRange
        {
            get => (TimeRange?)this.GetValue(TimeRangeProperty);
            protected set => this.SetValue(TimeRangeProperty, value);
        }

        public FloatRange? PriceRange
        {
            get => (FloatRange?)this.GetValue(PriceRangeProperty);
            protected set => this.SetValue(PriceRangeProperty, value);
        }

        public int? MaxVolume
        {
            get => (int?)this.GetValue(MaxVolumeProperty);
            protected set => this.SetValue(MaxVolumeProperty, value);
        }

        protected override int VisualChildrenCount => this.Children.Count;

        protected override IEnumerator LogicalChildren => this.Children.GetEnumerator();

        protected override Visual GetVisualChild(int index) => this.Children[index];

        protected override Size MeasureOverride(Size availableSize)
        {
            var rect = Rect.Empty;
            foreach (UIElement child in this.Children)
            {
                child.Measure(availableSize);
                rect.Union(new Rect(child.DesiredSize));
            }

            return rect.Size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            foreach (UIElement child in this.Children)
            {
                child.Arrange(new Rect(finalSize));
            }

            this.Refresh();
            return finalSize;
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (this.ItemsSource is { } candles &&
               Delta() is var delta &&
               delta != 0)
            {
                this.SetCurrentValue(
                    TimeProperty,
                    candles.Skip(
                        this.Time,
                        this.CandleInterval,
                        delta));
            }

            int Delta()
            {
                if (IsFromTouch(e))
                {
                    if (Math.Abs(e.Delta) < 4)
                    {
                        return Math.Sign(e.Delta);
                    }

                    return e.Delta switch
                    {
                        < 0 => Math.Min(-1, TouchDelta()),
                        > 0 => Math.Max(1, TouchDelta()),
                        _ => 0,
                    };

                    // Pan about the same length horizontally as the swipe
                    int TouchDelta() => e.Delta / this.CandleWidth;
                }

                // We try to calculate a step based on how fast user is spinning the wheel.
                return Scroll.DeltaTime(e) switch
                {
                    <= 0 => 0,
                    > 50 => Math.Sign(e.Delta),
                    var dt => e.Delta switch
                    {
                        < 0 => Math.Min(-1, -120 / dt),
                        > 0 => Math.Max(1, 240 / dt),
                        _ => 0,
                    },
                };

                // Must be better ways for this but may require pinvoke. Good enough for now.
                static bool IsFromTouch(MouseWheelEventArgs e) => e.Delta % Mouse.MouseWheelDeltaForOneLine != 0;
            }
        }

        private void Refresh()
        {
            var candles = this.Candles;
            candles.Refresh(this.ItemsSource, this.Time, this.CandleInterval);
            this.SetCurrentValue(TimeRangeProperty, candles.TimeRange);
            this.SetCurrentValue(PriceRangeProperty, candles.PriceRange);
            this.SetCurrentValue(MaxVolumeProperty, candles.MaxVolume);
        }

        private static class Scroll
        {
            private static int lastTimeStamp;

            internal static int DeltaTime(MouseWheelEventArgs e)
            {
                var delta = e.Timestamp - lastTimeStamp;
                lastTimeStamp = e.Timestamp;
                return delta;
            }
        }
    }
}
