namespace Sideways
{
    using System;
    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;
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
                FrameworkPropertyMetadataOptions.Inherits | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

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

        public UIElementCollection Children { get; }

        protected override int VisualChildrenCount => this.Children.Count;

        protected override IEnumerator LogicalChildren => this.Children.GetEnumerator();

        /// <summary>Helper for getting <see cref="TimeProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to read <see cref="TimeProperty"/> from.</param>
        /// <returns>Time property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static DateTimeOffset GetTime(DependencyObject element)
        {
            return (DateTimeOffset)element.GetValue(TimeProperty);
        }

        /// <summary>Helper for setting <see cref="TimeProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="TimeProperty"/> on.</param>
        /// <param name="value">Time property value.</param>
        public static void SetTime(DependencyObject element, DateTimeOffset value)
        {
            element.SetValue(TimeProperty, value);
        }

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
    }
}
