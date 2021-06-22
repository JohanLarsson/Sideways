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
    public class CrossHairDecorator : FrameworkElement
    {
        /// <summary>Identifies the <see cref="Brush"/> dependency property.</summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(SolidColorBrush),
            typeof(CrossHairDecorator),
            new FrameworkPropertyMetadata(
                Brushes.CrossHair,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (o, _) => ((CrossHairDecorator)o).pen = null));

        /// <summary>Identifies the <see cref="Position"/> dependency property.</summary>
        public static readonly DependencyProperty PositionProperty = DependencyProperty.Register(
            nameof(Position),
            typeof(Point?),
            typeof(CrossHairDecorator),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        private Pen? pen;
        private UIElement? child;

        static CrossHairDecorator()
        {
            ClipToBoundsProperty.OverrideMetadata(typeof(CrossHairDecorator), new PropertyMetadata(true));
        }

#pragma warning disable WPF0012 // CLR property type should match registered type.
        public SolidColorBrush? Brush
#pragma warning restore WPF0012 // CLR property type should match registered type.
        {
            get => (SolidColorBrush?)this.GetValue(BrushProperty);
            set => this.SetValue(BrushProperty, value);
        }

        public Point? Position
        {
            get => (Point?)this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
        }

        [DefaultValue(null)]
        public virtual UIElement? Child
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
            if (this.Brush is { } brush &&
                this.IsMouseOver)
            {
                this.pen ??= CreatePen(brush);
                var p = Mouse.GetPosition(this);
                drawingContext.DrawLine(this.pen, new Point(0, p.Y), new Point(renderSize.Width, p.Y));
                drawingContext.DrawLine(this.pen, new Point(p.X, 0), new Point(p.X, renderSize.Height));
            }
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.SetCurrentValue(PositionProperty, e.GetPosition(this));
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.SetCurrentValue(PositionProperty, null);
        }

        private static Pen? CreatePen(SolidColorBrush? brush)
        {
            if (brush is { })
            {
                var temp = new Pen(brush, 0.25);
                temp.Freeze();
                return temp;
            }

            return null;
        }
    }
}
