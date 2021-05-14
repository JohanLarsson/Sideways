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

        public Chart()
        {
            this.Children = new UIElementCollection(this, this);
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
