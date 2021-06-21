namespace Sideways
{
    using System;
    using System.Collections;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class MeasureAdorner : Adorner
    {
        private static readonly DependencyPropertyKey PositionPropertyKey = DependencyProperty.RegisterReadOnly(
            nameof(Position),
            typeof(Point),
            typeof(MeasureAdorner),
            new FrameworkPropertyMetadata(
                default(Point),
                FrameworkPropertyMetadataOptions.AffectsArrange));

        /// <summary>Identifies the <see cref="Position"/> dependency property.</summary>
        public static readonly DependencyProperty PositionProperty = PositionPropertyKey.DependencyProperty;

        private readonly Border child;

        private MeasureAdorner(UIElement adornedElement)
            : base(adornedElement)
        {
            if (adornedElement is null)
            {
                throw new ArgumentNullException(nameof(adornedElement));
            }

            this.child = new Border
            {
                Child = new ContentPresenter(),
                Background = Brushes.SemiTransparentBackground,
            };
            this.child.SetValue(TextElement.ForegroundProperty, Brushes.EnabledText);
            this.AddLogicalChild(this.child);
            this.AddVisualChild(this.child);
        }

        public Measurement Measurement
        {
            get => (Measurement)this.ContentPresenter.Content;
#pragma warning disable WPF0041 // Set mutable dependency properties using SetCurrentValue.
            private set => this.ContentPresenter.Content = value;
#pragma warning restore WPF0041 // Set mutable dependency properties using SetCurrentValue.
        }

        public Point Position
        {
            get => (Point)this.GetValue(PositionProperty);
            private set => this.SetValue(PositionPropertyKey, value);
        }

        protected override IEnumerator LogicalChildren => new SingleChildEnumerator(this.child);

        protected override int VisualChildrenCount => 1;

        private ContentPresenter ContentPresenter => (ContentPresenter)this.child.Child;

        public static MeasureAdorner? Show(MeasureDecorator measureDecorator, Measurement measurement, Point position)
        {
            var adorner = new MeasureAdorner(measureDecorator)
            {
                Measurement = measurement,
                Position = position,
            };

            AdornerService.Show(adorner);
            return adorner;
        }

        public void Update(Measurement measurement, Point position)
        {
            this.Measurement = measurement;
            this.Position = position;
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, "Check VisualChildrenCount first");
            }

            return this.child;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            this.child.Measure(constraint);
            return this.child.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var desiredSize = this.child.DesiredSize;
            this.child.Arrange(new Rect(new Point(this.Position.X - desiredSize.Width, this.Position.Y + OffsetY()), desiredSize));
            return finalSize;

            double OffsetY()
            {
                return this.Measurement switch
                {
                    { Start: { Price: var s }, End: { Price: var e } }
                        when s <= e
                        => -desiredSize.Height,
                    _ => 0,
                };
            }
        }
    }
}
