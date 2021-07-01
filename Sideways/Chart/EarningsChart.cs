namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Windows;
    using System.Windows.Media;

    public class EarningsChart : AbstractBarChart
    {
        public static readonly DependencyProperty EarningsProperty = EarningsBar.EarningsProperty.AddOwner(
            typeof(EarningsChart),
            new FrameworkPropertyMetadata(
                default(ImmutableArray<QuarterlyEarning>),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public ImmutableArray<QuarterlyEarning> Earnings
        {
            get => (ImmutableArray<QuarterlyEarning>)this.GetValue(EarningsProperty);
            set => this.SetValue(EarningsProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return this.Earnings switch
            {
                { IsDefaultOrEmpty: false, Length: var length } => new(Math.Min(this.Bars, length) * this.BarWidth, 0),
                _ => default,
            };
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderSize = this.RenderSize;
            if (renderSize is { Width: > 0, Height: > 0 } &&
                this.Earnings is { IsDefaultOrEmpty: false } earnings)
            {
                var min = float.MaxValue;
                var max = float.MinValue;
                for (var i = 0; i < Math.Min(this.Bars, earnings.Length); i++)
                {
                    min = Math.Min(min, earnings[i].ReportedEps);
                    max = Math.Max(max, earnings[i].ReportedEps);
                }

                var position = CandlePosition.RightToLeft(renderSize, this.BarWidth, new ValueRange(new FloatRange(Math.Min(0, min), Math.Max(0, max)), Scale.Arithmetic), 1, 1);
                for (var i = 0; i < Math.Min(this.Bars, earnings.Length); i++)
                {
                    drawingContext.DrawRectangle(
                        Brushes.LightGray,
                        null,
                        new Rect(
                            new Point(position.Left, position.Y(0)),
                            new Point(position.Right, position.Y(earnings[i].ReportedEps))));
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
