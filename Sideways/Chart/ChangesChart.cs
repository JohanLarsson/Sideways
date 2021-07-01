namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Windows;
    using System.Windows.Media;

    public class ChangesChart : AbstractBarChart
    {
        public static readonly DependencyProperty ChangesProperty = DependencyProperty.Register(
            nameof(Changes),
            typeof(ImmutableArray<Percent>),
            typeof(ChangesChart),
            new FrameworkPropertyMetadata(
                default(ImmutableArray<Percent>),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public ImmutableArray<Percent> Changes
        {
            get => (ImmutableArray<Percent>)this.GetValue(ChangesProperty);
            set => this.SetValue(ChangesProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return this.Changes switch
            {
                { IsDefaultOrEmpty: false, Length: var length } => new(Math.Min(this.Bars, length) * this.BarWidth, 0),
                _ => default,
            };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderSize = this.RenderSize;
            if (renderSize is { Width: > 0, Height: > 0 } &&
                this.Changes is { IsDefaultOrEmpty: false } changes)
            {
                var min = 0f;
                var max = 0f;
                for (var i = 0; i < Math.Min(this.Bars, changes.Length); i++)
                {
                    min = Math.Min(min, changes[i].Scalar);
                    max = Math.Max(max, changes[i].Scalar);
                }

                var position = CandlePosition.RightToLeft(renderSize, this.BarWidth, new ValueRange(new FloatRange(min, max), Scale.Arithmetic), 1, 1);

                for (var i = 0; i < Math.Min(this.Bars, changes.Length); i++)
                {
                    drawingContext.DrawRectangle(
                        ChangeToBrushConverter.Brush(changes[i]),
                        null,
                        new Rect(
                            new Point(position.Left, position.Y(0)),
                            new Point(position.Right, position.Y(changes[i].Scalar))));
                    position = position.ShiftLeft();
                    if (position.Left < 0)
                    {
                        break;
                    }
                }
            }
        }
    }
}
