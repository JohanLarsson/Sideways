namespace Sideways
{
    using System;
    using System.Collections;
    using System.Collections.Immutable;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class EarningsBar : FrameworkElement
    {
        /// <summary>Identifies the <see cref="Earnings"/> dependency property.</summary>
        public static readonly DependencyProperty EarningsProperty = DependencyProperty.Register(
            nameof(Earnings),
            typeof(ImmutableArray<QuarterlyEarning>),
            typeof(EarningsBar),
            new PropertyMetadata(
                default(ImmutableArray<QuarterlyEarning>),
                (d, e) => ((EarningsBar)d).OnEarningsChanged((ImmutableArray<QuarterlyEarning>)e.NewValue)));

        /// <summary>Identifies the <see cref="Time"/> dependency property.</summary>
        public static readonly DependencyProperty TimeProperty = Chart.TimeProperty.AddOwner(
            typeof(EarningsBar),
            new FrameworkPropertyMetadata(
                DateTimeOffset.Now,
                FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>Identifies the <see cref="CandleInterval"/> dependency property.</summary>
        public static readonly DependencyProperty CandleIntervalProperty = Chart.CandleIntervalProperty.AddOwner(
            typeof(EarningsBar),
            new FrameworkPropertyMetadata(
                CandleInterval.None,
                FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>Identifies the <see cref="CandleWidth"/> dependency property.</summary>
        public static readonly DependencyProperty CandleWidthProperty = Chart.CandleWidthProperty.AddOwner(
            typeof(EarningsBar),
            new FrameworkPropertyMetadata(
                5,
                FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>Identifies the <see cref="Candles"/> dependency property.</summary>
        public static readonly DependencyProperty CandlesProperty = Chart.CandlesProperty.AddOwner(typeof(EarningsBar));

        private readonly UIElementCollection children;

        public EarningsBar()
        {
            this.children = new UIElementCollection(this, this);
        }

        public ImmutableArray<QuarterlyEarning> Earnings
        {
            get => (ImmutableArray<QuarterlyEarning>)this.GetValue(EarningsProperty);
            set => this.SetValue(EarningsProperty, value);
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

#pragma warning disable WPF0012 // CLR property type should match registered type.
        public DescendingCandles Candles
#pragma warning restore WPF0012 // CLR property type should match registered type.
        {
            get => (DescendingCandles)this.GetValue(CandlesProperty);
            set => this.SetValue(CandlesProperty, value);
        }

        protected override IEnumerator LogicalChildren => this.children.GetEnumerator();

        protected override int VisualChildrenCount => this.children.Count;

        protected override Visual GetVisualChild(int index) => this.children[index];

        protected override Size MeasureOverride(Size availableSize)
        {
            var rect = Rect.Empty;
            foreach (ContentPresenter child in this.children)
            {
                child.Measure(availableSize);
                rect.Union(new Rect(child.DesiredSize));
            }

            return rect.Size;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var candles = this.Candles;
            var renderSize = this.RenderSize;
            var candleWidth = this.CandleWidth;
            var candleInterval = this.CandleInterval;

            foreach (ContentPresenter child in this.children)
            {
                if (child is { Content: EarningsViewModel { Date: { } date } })
                {
                    if (CandlePosition.X(date, candles, renderSize.Width, candleWidth, candleInterval) is { } x)
                    {
                        child.Visibility = Visibility.Visible;
                        var childDesiredSize = child.DesiredSize;
                        child.Arrange(new Rect(new Point(x - (childDesiredSize.Width / 2), 0), childDesiredSize));
                    }
                    else
                    {
                        child.Visibility = Visibility.Collapsed;
                    }
                }
            }

            return finalSize;
        }

        private void OnEarningsChanged(ImmutableArray<QuarterlyEarning> newValue)
        {
            this.children.Clear();
            if (!newValue.IsDefaultOrEmpty)
            {
                for (var i = 0; i < newValue.Length; i++)
                {
                    this.children.Add(new ContentPresenter { Content = new EarningsViewModel(newValue, i) });
                }
            }
        }
    }
}
