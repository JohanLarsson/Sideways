namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class TimeTickBar : CandleSeries
    {
        /// <summary>Identifies the <see cref="Fill"/> dependency property.</summary>
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill),
            typeof(SolidColorBrush),
            typeof(TimeTickBar),
            new FrameworkPropertyMetadata(
                Brushes.Gray,
                FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Fill property
        /// </summary>
        public SolidColorBrush Fill
        {
            get => (SolidColorBrush)this.GetValue(FillProperty);
            set => this.SetValue(FillProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new(25, TextElement.GetFontSize(this) * TextElement.GetFontFamily(this).LineSpacing);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var candles = this.Candles;
            if (candles is { Count: > 10 })
            {
                var candleWidth = this.CandleWidth;
                var actualWidth = this.ActualWidth;
                var typeface = new Typeface(
                    TextElement.GetFontFamily(this),
                    TextElement.GetFontStyle(this),
                    TextElement.GetFontWeight(this),
                    TextElement.GetFontStretch(this));
                var fontSize = TextElement.GetFontSize(this);
                var fill = this.Fill;

                switch (this.CandleInterval)
                {
                    case CandleInterval.Week:
                        break;
                    case CandleInterval.Day:
                        var max = Math.Min(candles.Count, actualWidth / candleWidth) - 1;
                        for (var i = 5; i < max; i++)
                        {
                            if (candles[i].Time.Month != candles[i + 1].Time.Month)
                            {
                                DrawText(TickText(candles[i].Time), actualWidth - (i * candleWidth));

                                static string TickText(DateTimeOffset time)
                                {
                                    return time switch
                                    {
                                        { Month: 1 } => time.Year.ToString(CultureInfo.InvariantCulture),
                                        _ => time.ToString("MMM", CultureInfo.InvariantCulture),
                                    };
                                }
                            }
                        }

                        break;
                    case CandleInterval.Hour:
                        break;
                    case CandleInterval.Minute:
                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }

                void DrawText(string text, double x)
                {
                    var formattedText = new FormattedText(
                        text,
                        CultureInfo.InvariantCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        fontSize,
                        fill,
                        96);
                    drawingContext.DrawText(
                        formattedText,
                        new Point(x, 0));
                }
            }
        }
    }
}
