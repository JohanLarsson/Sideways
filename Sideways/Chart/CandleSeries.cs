namespace Sideways
{
    using System;
    using System.Windows;

    public abstract class CandleSeries : FrameworkElement
    {
        /// <summary>Identifies the <see cref="ItemsSource"/> dependency property.</summary>
        public static readonly DependencyProperty ItemsSourceProperty = Chart.ItemsSourceProperty.AddOwner(
            typeof(CandleSeries),
            new FrameworkPropertyMetadata(
                default(Candles),
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="Time"/> dependency property.</summary>
        public static readonly DependencyProperty TimeProperty = Chart.TimeProperty.AddOwner(
            typeof(CandleSeries),
            new FrameworkPropertyMetadata(
                DateTimeOffset.Now,
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>Identifies the <see cref="CandleInterval"/> dependency property.</summary>
        public static readonly DependencyProperty CandleIntervalProperty = Chart.CandleIntervalProperty.AddOwner(
            typeof(CandleSeries),
            new FrameworkPropertyMetadata(
                CandleInterval.None,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="CandleWidth"/> dependency property.</summary>
        public static readonly DependencyProperty CandleWidthProperty = Chart.CandleWidthProperty.AddOwner(
            typeof(CandleSeries),
            new FrameworkPropertyMetadata(
                5,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="Candles"/> dependency property.</summary>
        public static readonly DependencyProperty CandlesProperty = Chart.CandlesProperty.AddOwner(typeof(CandleSeries));

        static CandleSeries()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(CandleSeries), new PropertyMetadata(true));
        }

        public Candles? ItemsSource
        {
            get => (Candles?)this.GetValue(ItemsSourceProperty);
            set => this.SetValue(ItemsSourceProperty, value);
        }

#pragma warning disable WPF0012 // CLR property type should match registered type.
        public DescendingCandles Candles
#pragma warning restore WPF0012 // CLR property type should match registered type.
        {
            get => (DescendingCandles)this.GetValue(CandlesProperty);
            set => this.SetValue(CandlesProperty, value);
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
