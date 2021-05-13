namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Windows;
    using System.Windows.Media;

    public abstract class CandleSeries : FrameworkElement
    {
        /// <summary>Identifies the <see cref="ItemsSource"/> dependency property.</summary>
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(Candles),
            typeof(CandleSeries),
            new FrameworkPropertyMetadata(
                default(Candles),
                FrameworkPropertyMetadataOptions.AffectsRender));

        private static readonly DependencyPropertyKey VisibleCandlesPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(VisibleCandles),
            typeof(ImmutableArray<Candle>?),
            typeof(CandleSeries),
            new PropertyMetadata(default(ImmutableArray<Candle>?)));

        /// <summary>Identifies the <see cref="VisibleCandles"/> dependency property.</summary>
        public static readonly DependencyProperty VisibleCandlesProperty = VisibleCandlesPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey PriceRangePropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(PriceRange),
            typeof(FloatRange?),
            typeof(CandleSeries),
            new PropertyMetadata(default(FloatRange?)));

        /// <summary>Identifies the <see cref="PriceRange"/> dependency property.</summary>
        public static readonly DependencyProperty PriceRangeProperty = PriceRangePropertyKey.DependencyProperty;

        /// <summary>Identifies the <see cref="Time"/> dependency property.</summary>
        public static readonly DependencyProperty TimeProperty = Chart.TimeProperty.AddOwner(
            typeof(CandleSeries),
            new FrameworkPropertyMetadata(
                DateTimeOffset.Now,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>Identifies the <see cref="CandleInterval"/> dependency property.</summary>
        public static readonly DependencyProperty CandleIntervalProperty = DependencyProperty.Register(
            nameof(CandleInterval),
            typeof(CandleInterval),
            typeof(CandleSeries),
            new PropertyMetadata(CandleInterval.None));

        /// <summary>Identifies the <see cref="CandleWidth"/> dependency property.</summary>
        public static readonly DependencyProperty CandleWidthProperty = DependencyProperty.Register(
            nameof(CandleWidth),
            typeof(int),
            typeof(CandleSeries),
            new FrameworkPropertyMetadata(
                5,
                FrameworkPropertyMetadataOptions.AffectsRender));

        static CandleSeries()
        {
            RenderOptions.EdgeModeProperty.OverrideMetadata(typeof(CandleSeries), new UIPropertyMetadata(EdgeMode.Aliased));
        }

        public Candles? ItemsSource
        {
            get => (Candles?)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

        public ImmutableArray<Candle>? VisibleCandles
        {
            get => (ImmutableArray<Candle>?)this.GetValue(VisibleCandlesProperty);
            protected set => this.SetValue(VisibleCandlesPropertyKey, value);
        }

        public FloatRange? PriceRange
        {
            get => (FloatRange?)this.GetValue(PriceRangeProperty);
            protected set => this.SetValue(PriceRangePropertyKey, value);
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
    }
}
