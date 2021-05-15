namespace Sideways
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
                FrameworkPropertyMetadataOptions.AffectsRender));

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
            return new(30, TextElement.GetFontSize(this) * TextElement.GetFontFamily(this).LineSpacing);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.Range is { } range)
            {
                var actualHeight = this.ActualHeight;
                var typeface = new Typeface(
                    TextElement.GetFontFamily(this),
                    TextElement.GetFontStyle(this),
                    TextElement.GetFontWeight(this),
                    TextElement.GetFontStretch(this));
                var fontSize = TextElement.GetFontSize(this);
                var fill = this.Fill;

                var step = (range.Max - range.Min) switch
                {
                    < 10 => 1,
                    < 100 => 10,
                    < 1000 => 100,
                    < 10000 => 1000,
                };

                var value = range.Min + step - (range.Min % step);
                while (value < range.Max)
                {
                    DrawText(value.ToString(CultureInfo.CurrentUICulture), range.Y(value, actualHeight));
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
    }
}
