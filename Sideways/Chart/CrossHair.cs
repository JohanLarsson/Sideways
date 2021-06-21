namespace Sideways
{
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
                (d, e) => ((CrossHair)d).pen = CreatePen((SolidColorBrush?)e.NewValue)));

        /// <summary>Identifies the <see cref="PriceRange"/> dependency property.</summary>
        public static readonly DependencyProperty PriceRangeProperty = Chart.PriceRangeProperty.AddOwner(
            typeof(CrossHair),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="PriceScale"/> dependency property.</summary>
        public static readonly DependencyProperty PriceScaleProperty = Chart.PriceScaleProperty.AddOwner(
            typeof(CrossHair),
            new FrameworkPropertyMetadata(
                Scale.Logarithmic,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="Candles"/> dependency property.</summary>
        public static readonly DependencyProperty CandlesProperty = Chart.CandlesProperty.AddOwner(typeof(CrossHair));

        /// <summary>Identifies the <see cref="CandleInterval"/> dependency property.</summary>
        public static readonly DependencyProperty CandleIntervalProperty = Chart.CandleIntervalProperty.AddOwner(
            typeof(CrossHair),
            new FrameworkPropertyMetadata(
                CandleInterval.None,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="CandleWidth"/> dependency property.</summary>
        public static readonly DependencyProperty CandleWidthProperty = Chart.CandleWidthProperty.AddOwner(
            typeof(CrossHair),
            new FrameworkPropertyMetadata(
                5,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="Position"/> dependency property.</summary>
        public static readonly DependencyProperty PositionProperty = DependencyProperty.RegisterAttached(
            nameof(Position),
            typeof(CrossHairPosition?),
            typeof(CrossHair),
            new FrameworkPropertyMetadata(
                default(CrossHairPosition?),
                FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        private Pen? pen;

        static CrossHair()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CrossHair), new FrameworkPropertyMetadata(typeof(CrossHair)));
            ClipToBoundsProperty.OverrideMetadata(typeof(CrossHair), new PropertyMetadata(true));
        }

        public SolidColorBrush? Brush
        {
            get => (SolidColorBrush?)this.GetValue(BrushProperty);
            set => this.SetValue(BrushProperty, value);
        }

#pragma warning disable WPF0012 // CLR property type should match registered type.
        public DescendingCandles Candles
#pragma warning restore WPF0012 // CLR property type should match registered type.
        {
            get => (DescendingCandles)this.GetValue(CandlesProperty);
            set => this.SetValue(CandlesProperty, value);
        }

        public int CandleWidth
        {
            get => (int)this.GetValue(CandleWidthProperty);
            set => this.SetValue(CandleWidthProperty, value);
        }

        public CandleInterval CandleInterval
        {
            get => (CandleInterval)this.GetValue(CandleIntervalProperty);
            set => this.SetValue(CandleIntervalProperty, value);
        }

        public FloatRange? PriceRange
        {
            get => (FloatRange?)this.GetValue(PriceRangeProperty);
            set => this.SetValue(PriceRangeProperty, value);
        }

        public Scale PriceScale
        {
            get => (Scale)this.GetValue(PriceScaleProperty);
            set => this.SetValue(PriceScaleProperty, value);
        }

        public CrossHairPosition? Position
        {
            get => (CrossHairPosition?)this.GetValue(PositionProperty);
            set => this.SetValue(PositionProperty, value);
        }

        /// <summary>Helper for getting <see cref="PositionProperty"/> from <paramref name="e"/>.</summary>
        /// <param name="e"><see cref="UIElement"/> to read <see cref="PositionProperty"/> from.</param>
        /// <returns>Position property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(UIElement))]
        public static CrossHairPosition? GetPosition(UIElement e) => (CrossHairPosition?)e.GetValue(PositionProperty);

        /// <summary>Helper for setting <see cref="PositionProperty"/> on <paramref name="e"/>.</summary>
        /// <param name="e"><see cref="UIElement"/> to set <see cref="PositionProperty"/> on.</param>
        /// <param name="position">Position property value.</param>
        public static void SetPosition(UIElement e, CrossHairPosition? position) => e.SetValue(PositionProperty, position);

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderSize = this.RenderSize;
            if (this.pen is { } &&
                this.PriceRange is { } priceRange &&
                this.Position is { Price: var price })
            {
                if (this.IsMouseOver)
                {
                    var p = Mouse.GetPosition(this);
                    drawingContext.DrawLine(this.pen, new Point(0, p.Y), new Point(renderSize.Width, p.Y));
                    drawingContext.DrawLine(this.pen, new Point(p.X, 0), new Point(p.X, renderSize.Height));
                }
                else
                {
                    var y = new ValueRange(priceRange, this.PriceScale).Y(price, renderSize.Height);
                    drawingContext.DrawLine(this.pen, new Point(0, y), new Point(renderSize.Width, y));
                    //// context.DrawLine(this.pen, new Point(p.X, 0), new Point(p.X, size.Height));
                }
            }
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.PriceRange is { } priceRange &&
                CandlePosition.RightToLeft(this.RenderSize, this.CandleWidth, new ValueRange(priceRange, this.PriceScale)).TimeAndPrice(e.GetPosition(this), this.Candles) is { Time: var time, Price: var price })
            {
                this.SetCurrentValue(PositionProperty, new CrossHairPosition(time, price, this.CandleInterval));
            }
            else
            {
                this.SetCurrentValue(PositionProperty, null);
            }
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            this.SetCurrentValue(PositionProperty, null);
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
