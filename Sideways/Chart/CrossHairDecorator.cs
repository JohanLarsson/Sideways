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
        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            nameof(Stroke),
            typeof(SolidColorBrush),
            typeof(CrossHairDecorator),
            new FrameworkPropertyMetadata(
                Brushes.CrossHair,
                FrameworkPropertyMetadataOptions.AffectsRender,
                (o, _) => ((CrossHairDecorator)o).pen = null));

        private static readonly DependencyPropertyKey PositionPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Position),
            typeof(Point?),
            typeof(CrossHairDecorator),
            new PropertyMetadata(null));

        public static readonly DependencyProperty PositionProperty = PositionPropertyKey.DependencyProperty;

        private readonly OverlayVisual overlay = new();
        private Pen? pen;
        private UIElement? child;

        public CrossHairDecorator()
        {
            this.AddVisualChild(this.overlay);
        }

#pragma warning disable WPF0012 // CLR property type should match registered type.
        public SolidColorBrush? Stroke
#pragma warning restore WPF0012 // CLR property type should match registered type.
        {
            get => (SolidColorBrush?)this.GetValue(StrokeProperty);
            set => this.SetValue(StrokeProperty, value);
        }

        public Point? Position
        {
            get => (Point?)this.GetValue(PositionProperty);
            set => this.SetValue(PositionPropertyKey, value);
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

        protected override int VisualChildrenCount => this.child is null ? 0 : 2;

        protected override Visual GetVisualChild(int index)
        {
            return index switch
            {
                0 when this.child is { } => this.child,
                1 when this.child is { } => this.overlay,
                _ => throw new ArgumentOutOfRangeException(nameof(index), index, "Check VisualChildrenCount first"),
            };
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
            this.Render(this.Position);
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            var position = e.GetPosition(this);
            this.Render(position);
            this.Position = position;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.Render(null);
            this.Position = null;
        }

        private void Render(Point? position)
        {
            using var drawingContext = this.overlay.RenderOpen();
            if (this.Stroke is { } stroke &&
                position is { } p)
            {
                var renderSize = this.RenderSize;
                this.pen ??= CreatePen(stroke);
                drawingContext.DrawLine(this.pen, new Point(0, p.Y), new Point(renderSize.Width, p.Y));
                drawingContext.DrawLine(this.pen, new Point(p.X, 0), new Point(p.X, renderSize.Height));

                static Pen CreatePen(SolidColorBrush brush)
                {
                    var temp = new Pen(brush, 0.25);
                    temp.Freeze();
                    return temp;
                }
            }
        }
    }
}
