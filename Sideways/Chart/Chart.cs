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
        /// <summary>Identifies the <see cref="Background"/> dependency property.</summary>
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background),
            typeof(Brush),
            typeof(Chart),
            new PropertyMetadata(System.Windows.Media.Brushes.Transparent));

        /// <summary>Identifies the <see cref="Time"/> dependency property.</summary>
        public static readonly DependencyProperty TimeProperty = DependencyProperty.RegisterAttached(
            nameof(Time),
            typeof(DateTimeOffset),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                default(DateTimeOffset),
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsArrange));

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
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>Identifies the <see cref="CandleWidth"/> dependency property.</summary>
        public static readonly DependencyProperty CandleWidthProperty = DependencyProperty.RegisterAttached(
            nameof(CandleWidth),
            typeof(int),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                5,
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>Identifies the <see cref="PriceRange"/> dependency property.</summary>
        public static readonly DependencyProperty PriceRangeProperty = DependencyProperty.RegisterAttached(
            nameof(PriceRange),
            typeof(FloatRange?),
            typeof(Chart),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.Inherits));

        private readonly List<Candle> candles = new();

        public Chart()
        {
            this.Children = new UIElementCollection(this, this);
            this.Candles = this.candles;
        }

        public Brush Background
        {
            get => (Brush)this.GetValue(BackgroundProperty);
            set => this.SetValue(BackgroundProperty, value);
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
                var visible = (int)Math.Ceiling(finalSize.Width / this.CandleWidth);
                foreach (var candle in itemsSource.Get(this.Time, this.CandleInterval)
                                                  .Take(visible + this.ExtraCandles))
                {
                    if (this.candles.Count <= visible)
                    {
                        min = Math.Min(min, candle.Low);
                        max = Math.Max(max, candle.High);
                    }

                    this.candles.Add(candle);
                }

                this.SetCurrentValue(PriceRangeProperty, new FloatRange(min, max));
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
            if (this.Background is { } background)
            {
                drawingContext.DrawRectangle(
                    background,
                    null,
                    new Rect(this.RenderSize));
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if (this.ItemsSource is { } candles)
            {
                this.SetCurrentValue(
                    TimeProperty,
                    candles.Skip(
                        this.Time,
                        this.CandleInterval,
                        Delta()));

                int Delta()
                {
                    if (e.StylusDevice is { })
                    {
                        return Math.Sign(e.Delta) * Math.Max(1, Math.Abs(e.Delta) / this.CandleWidth);
                    }

                    return Math.Sign(e.Delta);
                }
            }
        }
    }
}
