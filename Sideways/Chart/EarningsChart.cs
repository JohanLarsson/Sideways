namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Windows;
    using System.Windows.Media;

    public class EarningsChart : FrameworkElement
    {
        public static readonly DependencyProperty EarningsProperty = EarningsBar.EarningsProperty.AddOwner(
            typeof(EarningsChart),
            new FrameworkPropertyMetadata(
                default(ImmutableArray<QuarterlyEarning>),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BarWidthProperty = DependencyProperty.Register(
            nameof(BarWidth),
            typeof(int),
            typeof(EarningsChart),
            new FrameworkPropertyMetadata(
                5,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BarsProperty = DependencyProperty.Register(
            nameof(Bars),
            typeof(int),
            typeof(EarningsChart),
            new FrameworkPropertyMetadata(
                16,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public ImmutableArray<QuarterlyEarning> Earnings
        {
            get => (ImmutableArray<QuarterlyEarning>)this.GetValue(EarningsProperty);
            set => this.SetValue(EarningsProperty, value);
        }

        public int BarWidth
        {
            get => (int)this.GetValue(BarWidthProperty);
            set => this.SetValue(BarWidthProperty, value);
        }

        public int Bars
        {
            get => (int)this.GetValue(BarsProperty);
            set => this.SetValue(BarsProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new(this.Bars * this.BarWidth, 0);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderSize = this.RenderSize;
            if (renderSize is { Width: > 0, Height: > 0 } &&
                this.Earnings is { IsDefaultOrEmpty: false } earnings)
            {
                var max = float.MinValue;
                var min = float.MaxValue;
                for (var i = 0; i < Math.Min(this.Bars, earnings.Length); i++)
                {
                    max = Math.Max(max, earnings[i].ReportedEps);
                    min = Math.Min(min, earnings[i].ReportedEps);
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
