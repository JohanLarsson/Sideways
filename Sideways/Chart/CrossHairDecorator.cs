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
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PositionProperty = PositionPropertyKey.DependencyProperty;

        private Pen? pen;
        private UIElement? child;

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
            if (this.Stroke is { } brush &&
                this.Position is { } p)
            {
                this.pen ??= CreatePen(brush);
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

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.Position = e.GetPosition(this);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.Position = null;
        }
    }
}
