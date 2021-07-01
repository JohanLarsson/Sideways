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

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderSize = this.RenderSize;
            if (renderSize is { Width: > 0, Height: > 0 } &&
                this.Changes is { IsDefaultOrEmpty: false } changes)
            {
                var max = float.MinValue;
                var min = float.MaxValue;
                for (var i = 0; i < Math.Min(this.Bars, changes.Length); i++)
                {
                    max = Math.Max(max, changes[i].Scalar);
                    min = Math.Min(min, changes[i].Scalar);
                }

                var position = CandlePosition.RightToLeft(renderSize, this.BarWidth, new ValueRange(new FloatRange(Math.Min(0, min), Math.Max(0, max)), Scale.Arithmetic), 1, 1);
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
