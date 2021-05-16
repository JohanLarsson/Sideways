namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;

    public class CrossHair : FrameworkElement
    {
        /// <summary>Identifies the <see cref="Brush"/> dependency property.</summary>
        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            nameof(Brush),
            typeof(SolidColorBrush),
            typeof(CrossHair),
            new FrameworkPropertyMetadata(
                default(SolidColorBrush),
                FrameworkPropertyMetadataOptions.AffectsRender,
#pragma warning disable WPF0022 // Cast value to correct type.
                (d, e) => ((CrossHair)d).pen = CreatePen((SolidColorBrush?)e.NewValue)));
#pragma warning restore WPF0022 // Cast value to correct type.

        /// <summary>Identifies the <see cref="Range"/> dependency property.</summary>
        public static readonly DependencyProperty RangeProperty = Chart.RangeProperty.AddOwner(
            typeof(CrossHair),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="Range"/> dependency property.</summary>
        public static readonly DependencyProperty CandlesProperty = Chart.CandlesProperty.AddOwner(typeof(CrossHair));

        /// <summary>Identifies the <see cref="CandleWidth"/> dependency property.</summary>
        public static readonly DependencyProperty CandleWidthProperty = Chart.CandleWidthProperty.AddOwner(
            typeof(CrossHair),
            new FrameworkPropertyMetadata(
                5,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="TimeAndValue"/> dependency property.</summary>
        public static readonly DependencyProperty TimeAndValueProperty = DependencyProperty.Register(
            nameof(TimeAndValue),
            typeof(TimeAndValue?),
            typeof(CrossHair),
            new FrameworkPropertyMetadata(
                default(TimeAndValue?),
                FrameworkPropertyMetadataOptions.AffectsRender));

        private readonly DrawingVisual drawing;

        private Pen? pen;

        static CrossHair()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CrossHair), new FrameworkPropertyMetadata(typeof(CrossHair)));
            ClipToBoundsProperty.OverrideMetadata(typeof(CrossHair), new PropertyMetadata(true));
        }

        public CrossHair()
        {
            this.drawing = new DrawingVisual();
            this.AddVisualChild(this.drawing);
        }

#pragma warning disable WPF0012 // CLR property type should match registered type.
        public SolidColorBrush? Brush
#pragma warning restore WPF0012 // CLR property type should match registered type.
        {
            get => (SolidColorBrush?)this.GetValue(BrushProperty);
            set => this.SetValue(BrushProperty, value);
        }

#pragma warning disable WPF0012 // CLR property type should match registered type.
        public IReadOnlyList<Candle> Candles
#pragma warning restore WPF0012 // CLR property type should match registered type.
        {
            get => (IReadOnlyList<Candle>)this.GetValue(CandlesProperty);
            set => this.SetValue(CandlesProperty, value);
        }

        public int CandleWidth
        {
            get => (int)this.GetValue(CandleWidthProperty);
            set => this.SetValue(CandleWidthProperty, value);
        }

        public FloatRange? Range
        {
            get => (FloatRange?)this.GetValue(RangeProperty);
            set => this.SetValue(RangeProperty, value);
        }

        public TimeAndValue? TimeAndValue
        {
            get => (TimeAndValue?)this.GetValue(TimeAndValueProperty);
            set => this.SetValue(TimeAndValueProperty, value);
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index) => index == 0
            ? this.drawing
            : throw new ArgumentOutOfRangeException(nameof(index));

        protected override void OnRender(DrawingContext drawingContext)
        {
            var size = this.RenderSize;
            using var context = this.drawing.RenderOpen();
            context.DrawRectangle(
                System.Windows.Media.Brushes.Transparent,
                null,
                new Rect(this.RenderSize));
            if (this.pen is { } &&
                this.Range is { } priceRange)
            {
                var p = Mouse.GetPosition(this);
                context.DrawLine(this.pen, new Point(0, p.Y), new Point(size.Width, p.Y));
                context.DrawLine(this.pen, new Point(p.X, 0), new Point(p.X, size.Height));

                double Y(float price) => priceRange.Y(price, size.Height);
            }
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            var size = this.RenderSize;
            var pos = e.GetPosition(this);
            var i = (int)Math.Round((size.Width - pos.X) / this.CandleWidth);
            if (this.Candles.Count > i &&
                i >= 0 &&
                this.Range is { } range)
            {
                this.SetCurrentValue(TimeAndValueProperty, new TimeAndValue(this.Candles[i].Time, range.ValueFromY(pos.Y, size.Height)));
            }
            else
            {
                this.SetCurrentValue(TimeAndValueProperty, null);
            }

            base.OnPreviewMouseMove(e);
        }

        private static Pen? CreatePen(SolidColorBrush? brush)
        {
            if (brush is { })
            {
                var temp = new Pen(brush, 0.5);
                temp.Freeze();
                return temp;
            }

            return null;
        }
    }
}
