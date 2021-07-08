namespace Sideways
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class PriceTickBar : FrameworkElement
    {
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill),
            typeof(SolidColorBrush),
            typeof(PriceTickBar),
            new FrameworkPropertyMetadata(
                Brushes.Gray,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PriceRangeProperty = Chart.PriceRangeProperty.AddOwner(
            typeof(PriceTickBar),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PriceScaleProperty = Chart.PriceScaleProperty.AddOwner(
            typeof(PriceTickBar),
            new FrameworkPropertyMetadata(
                Scale.Logarithmic,
                FrameworkPropertyMetadataOptions.AffectsRender));

        private StepAndFormat stepAndFormat;

        public SolidColorBrush Fill
        {
            get => (SolidColorBrush)this.GetValue(FillProperty);
            set => this.SetValue(FillProperty, value);
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

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.PriceRange is { Max: var max } range)
            {
                this.stepAndFormat = StepAndFormat.Create(range, availableSize.Height);
                var text = new FormattedText(
                    max.ToString(this.stepAndFormat.Format, DateTimeFormatInfo.InvariantInfo),
                    CultureInfo.InvariantCulture,
                    FlowDirection.LeftToRight,
                    new Typeface(
                        TextElement.GetFontFamily(this),
                        TextElement.GetFontStyle(this),
                        TextElement.GetFontWeight(this),
                        TextElement.GetFontStretch(this)),
                    TextElement.GetFontSize(this),
                    this.Fill,
                    96);
                return new Size(text.Width, text.Height);
            }

            return default;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.PriceRange is { } range &&
                this.stepAndFormat is { Step: var step, Format: { } format })
            {
                var fontFamily = TextElement.GetFontFamily(this);
                var typeface = new Typeface(
                    fontFamily,
                    TextElement.GetFontStyle(this),
                    TextElement.GetFontWeight(this),
                    TextElement.GetFontStretch(this));
                var fontSize = TextElement.GetFontSize(this);
                var fill = this.Fill;
                var value = range.Min + step - (range.Min % step);
                var halfTextHeight = fontSize * fontFamily.LineSpacing / 2;
                var position = CandlePosition.RightToLeft(this.RenderSize, default, new ValueRange(range, this.PriceScale));
                while (value < range.Max)
                {
#pragma warning disable CA1305 // Specify IFormatProvider
                    DrawText(value.ToString(format, CultureInfo.CurrentUICulture), position.Y(value) - halfTextHeight);
#pragma warning restore CA1305 // Specify IFormatProvider
                    value += step;
                }

                void DrawText(string text, double y)
                {
                    drawingContext.DrawText(
                        new FormattedText(
                            text,
                            CultureInfo.CurrentUICulture,
                            FlowDirection.LeftToRight,
                            typeface,
                            fontSize,
                            fill,
                            96),
                        new Point(0, y));
                }
            }
        }

        private readonly struct StepAndFormat : IEquatable<StepAndFormat>
        {
            internal readonly float Step;
            internal readonly string Format;

            private StepAndFormat(float step, string format)
            {
                this.Step = step;
                this.Format = format;
            }

            public bool Equals(StepAndFormat other) => this.Step.Equals(other.Step) && this.Format == other.Format;

            public override bool Equals(object? obj) => obj is StepAndFormat other && this.Equals(other);

            public override int GetHashCode() => HashCode.Combine(this.Step, this.Format);

            internal static StepAndFormat Create(FloatRange priceRange, double height)
            {
                var step = Step(priceRange, height);
                return new StepAndFormat(step, StringFormat(step));

                static float Step(FloatRange range, double height)
                {
                    if (range.Max - range.Min < 0.0001)
                    {
                        return float.MaxValue;
                    }

                    var valueRange = new ValueRange(range, Scale.Arithmetic);
                    var step = 1.0f;
                    while (Pixels() < 50)
                    {
                        step *= 10;
                    }

                    while (Pixels() > 500)
                    {
                        step /= 10;
                    }

                    if (Pixels() > 100)
                    {
                        step *= 0.5f;
                    }

                    return step;

                    double Pixels() => valueRange.Y(range.Max - step, height);
                }

                static string StringFormat(float step)
                {
                    return step switch
                    {
                        < 0.001f => "F4",
                        < 0.01f => "F3",
                        < 0.1f => "F2",
                        < 1 => "F1",
                        _ => "F0",
                    };
                }
            }
        }
    }
}
