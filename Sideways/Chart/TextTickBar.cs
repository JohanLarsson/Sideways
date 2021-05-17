﻿namespace Sideways
{
    using System.Globalization;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class TextTickBar : FrameworkElement
    {
        /// <summary>Identifies the <see cref="Fill"/> dependency property.</summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill),
            typeof(SolidColorBrush),
            typeof(TextTickBar),
            new FrameworkPropertyMetadata(
                Brushes.Gray,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>Identifies the <see cref="Range"/> dependency property.</summary>
        public static readonly DependencyProperty RangeProperty = Chart.RangeProperty.AddOwner(
            typeof(TextTickBar),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsParentMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Fill property
        /// </summary>
        public SolidColorBrush Fill
        {
            get => (SolidColorBrush)this.GetValue(FillProperty);
            set => this.SetValue(FillProperty, value);
        }

        public FloatRange? Range
        {
            get => (FloatRange?)this.GetValue(RangeProperty);
            set => this.SetValue(RangeProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.Range is { Max: var max } range &&
                availableSize.Height is > 0 and < double.PositiveInfinity)
            {
                var text = new FormattedText(
                    max.ToString(StringFormat(Step(range, availableSize.Height)), DateTimeFormatInfo.InvariantInfo),
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

            return Size.Empty;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.Range is { } range)
            {
                var actualHeight = this.ActualHeight;
                var fontFamily = TextElement.GetFontFamily(this);
                var typeface = new Typeface(
                    fontFamily,
                    TextElement.GetFontStyle(this),
                    TextElement.GetFontWeight(this),
                    TextElement.GetFontStretch(this));
                var fontSize = TextElement.GetFontSize(this);
                var fill = this.Fill;

                var step = Step(range, actualHeight);
                var format = StringFormat(step);
                var value = range.Min + step - (range.Min % step);
                var halfTextHeight = fontSize * fontFamily.LineSpacing / 2;
                while (value < range.Max)
                {
#pragma warning disable CA1305 // Specify IFormatProvider
                    DrawText(value.ToString(format, CultureInfo.CurrentUICulture), range.Y(value, actualHeight) - halfTextHeight);
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

        private static float Step(FloatRange range, double height)
        {
            if (range.Max - range.Min < 0.0001)
            {
                return float.MaxValue;
            }

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

            double Pixels() => range.Y(range.Max - step, height);
        }

        private static string StringFormat(float step)
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
