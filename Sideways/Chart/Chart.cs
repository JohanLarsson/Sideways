namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Markup;
    using System.Windows.Media;

    [ContentProperty(nameof(Children))]
    public class Chart : FrameworkElement
    {
        /// <summary>Identifies the <see cref="Time"/> dependency property.</summary>
        public static readonly DependencyProperty TimeProperty = DependencyProperty.RegisterAttached(
            nameof(Time),
            typeof(DateTimeOffset),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                default(DateTimeOffset),
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="ItemsSource"/> dependency property.</summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.RegisterAttached(
            nameof(ItemsSource),
            typeof(Candles),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                default(Candles),
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>Identifies the <see cref="Candles"/> dependency property.</summary>
        public static readonly DependencyProperty CandlesProperty = DependencyProperty.RegisterAttached(
            nameof(Candles),
            typeof(IReadOnlyList<Candle>),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                default(IReadOnlyList<Candle>),
                FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>Identifies the <see cref="ExtraCandles"/> dependency property.</summary>
        public static readonly DependencyProperty ExtraCandlesProperty = DependencyProperty.Register(
            nameof(ExtraCandles),
            typeof(int),
            typeof(Chart),
            new PropertyMetadata(default(int)));

        /// <summary>Identifies the <see cref="CandleInterval"/> dependency property.</summary>
        public static readonly DependencyProperty CandleIntervalProperty = DependencyProperty.RegisterAttached(
            nameof(CandleInterval),
            typeof(CandleInterval),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                CandleInterval.None,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="CandleWidth"/> dependency property.</summary>
        public static readonly DependencyProperty CandleWidthProperty = DependencyProperty.RegisterAttached(
            nameof(CandleWidth),
            typeof(int),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                5,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="PriceRange"/> dependency property.</summary>
        public static readonly DependencyProperty PriceRangeProperty = DependencyProperty.RegisterAttached(
            nameof(PriceRange),
            typeof(FloatRange?),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.Inherits));

        /// <summary>Identifies the <see cref="MaxVolume"/> dependency property.</summary>
        public static readonly DependencyProperty MaxVolumeProperty = DependencyProperty.RegisterAttached(
            nameof(MaxVolume),
            typeof(int?),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                default(int?),
                FrameworkPropertyMetadataOptions.Inherits));

        private readonly List<Candle> candles = new();
        private int lastTimeStamp;

        public Chart()
        {
            this.Children = new UIElementCollection(this, this);
            this.Candles = this.candles;
        }

        public DateTimeOffset Time
        {
            get => (DateTimeOffset)this.GetValue(TimeProperty);
            set => this.SetValue(TimeProperty, value);
        }

        public Candles? ItemsSource
        {
            get => (Candles?)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

#pragma warning disable WPF0012 // CLR property type should match registered type.
        public IReadOnlyList<Candle> Candles
#pragma warning restore WPF0012 // CLR property type should match registered type.
        {
            get => (IReadOnlyList<Candle>)this.GetValue(CandlesProperty);
            set => this.SetValue(CandlesProperty, value);
        }

        public int ExtraCandles
        {
            get => (int)this.GetValue(ExtraCandlesProperty);
            set => this.SetValue(ExtraCandlesProperty, value);
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

        public UIElementCollection Children { get; }

        protected override int VisualChildrenCount => this.Children.Count;

        protected override IEnumerator LogicalChildren => this.Children.GetEnumerator();

        protected override Visual GetVisualChild(int index) => this.Children[index];

        protected override Size MeasureOverride(Size availableSize)
        {
            foreach (UIElement child in this.Children)
            {
                child.Measure(availableSize);
            }

            return base.MeasureOverride(availableSize);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            this.candles.Clear();
            if (finalSize.Width > 0 &&
                finalSize.Height > 0 &&
                this.ItemsSource is { } itemsSource)
            {
                var min = float.MaxValue;
                var max = float.MinValue;
                var maxVolume = 0;
                var visible = (int)Math.Ceiling(finalSize.Width / this.CandleWidth);
                foreach (var candle in itemsSource.Get(this.Time, this.CandleInterval)
                                                  .Take(visible + this.ExtraCandles))
                {
                    if (this.candles.Count <= visible)
                    {
                        min = Math.Min(min, candle.Low);
                        max = Math.Max(max, candle.High);
                        maxVolume = Math.Max(maxVolume, candle.Volume);
                    }

                    this.candles.Add(candle);
                }

                this.SetCurrentValue(PriceRangeProperty, new FloatRange(min, max));
                this.SetCurrentValue(MaxVolumeProperty, maxVolume);
            }
            else
            {
                this.SetCurrentValue(PriceRangeProperty, null);
            }

            foreach (UIElement child in this.Children)
            {
                child.Arrange(new Rect(finalSize));
            }

            return base.ArrangeOverride(finalSize);
        }

        /// <summary>
        ///     Fills in the background based on the Background property.
        /// </summary>
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawRectangle(
                System.Windows.Media.Brushes.Transparent,
                null,
                new Rect(this.RenderSize));
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            var dt = e.Timestamp - this.lastTimeStamp;
            //// Debug.WriteLine($"Timestamp: {e.Timestamp} DeltaTime: {dt} Delta:{e.Delta} Delta() {Delta()}");
            this.lastTimeStamp = e.Timestamp;
            if (this.ItemsSource is { } candles &&
               Delta() is var delta &&
               delta != 0)
            {
                this.SetCurrentValue(
                    TimeProperty,
                    candles.Skip(
                        this.Time,
                        this.CandleInterval,
                        Delta()));
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

                if (dt <= 0)
                {
                    return 0;
                }

                return dt switch
                {
                    <= 0 => 0,
                    > 50 => Math.Sign(e.Delta),
                    _ => e.Delta switch
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
    }
}
