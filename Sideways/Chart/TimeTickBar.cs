namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;

    public class TimeTickBar : CandlesElement
    {
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
            nameof(Fill),
            typeof(SolidColorBrush),
            typeof(TimeTickBar),
            new FrameworkPropertyMetadata(
                Brushes.Gray,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty TimeRangeProperty = Chart.TimeRangeProperty.AddOwner(
            typeof(TimeTickBar),
            new FrameworkPropertyMetadata(
                default(TimeRange?),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public SolidColorBrush Fill
        {
            get => (SolidColorBrush)this.GetValue(FillProperty);
            set => this.SetValue(FillProperty, value);
        }

        public TimeRange? TimeRange
        {
            get => (TimeRange?)this.GetValue(TimeRangeProperty);
            protected set => this.SetValue(TimeRangeProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new(25, TextElement.GetFontSize(this) * TextElement.GetFontFamily(this).LineSpacing);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (this.TimeRange is { } &&
                this.Candles is { Count: > 10 } candles)
            {
                var typeface = new Typeface(
                    TextElement.GetFontFamily(this),
                    TextElement.GetFontStyle(this),
                    TextElement.GetFontWeight(this),
                    TextElement.GetFontStretch(this));
                var fontSize = TextElement.GetFontSize(this);
                var fill = this.Fill;
                var position = CandlePosition.RightToLeft(this.RenderSize, this.CandleWidth, default);

                switch (this.CandleInterval)
                {
                    case CandleInterval.Week:
                        for (var i = 0; i < candles.Count - 1; i++)
                        {
                            if (candles[i].Time.Year != candles[i + 1].Time.Year)
                            {
                                DrawText(candles[i].Time.Year.ToString(CultureInfo.InvariantCulture), position.Left);
                            }

                            position = position.ShiftLeft();
                            if (position.Left < 0)
                            {
                                break;
                            }
                        }

                        break;
                    case CandleInterval.Day:
                        for (var i = 0; i < candles.Count - 1; i++)
                        {
                            if (candles[i].Time.Month != candles[i + 1].Time.Month)
                            {
                                DrawText(TickText(candles[i].Time), position.Left);

                                static string TickText(DateTimeOffset time)
                                {
                                    return time switch
                                    {
                                        { Month: 1 } => time.Year.ToString(CultureInfo.InvariantCulture),
                                        _ => time.ToString("MMM", CultureInfo.InvariantCulture),
                                    };
                                }
                            }

                            position = position.ShiftLeft();
                            if (position.Left < 0)
                            {
                                break;
                            }
                        }

                        break;
                    case CandleInterval.Hour:
                        foreach (var candle in candles)
                        {
                            if (HourAndMinute.EndOfHourCandle(candle.Time) == new HourAndMinute(10, 00))
                            {
                                DrawText(candle.Time.Day.ToString(CultureInfo.InvariantCulture), position.Left);
                            }

                            position = position.ShiftLeft();
                            if (position.Left < 0)
                            {
                                break;
                            }
                        }

                        break;
                    case CandleInterval.FifteenMinutes:
                        for (var i = 1; i < candles.Count; i++)
                        {
                            if (BeginHourCandle(i) is { } hourAndMinute &&
                                hourAndMinute is { Hour: 9, Minute: 30 } or { Hour: 12, Minute: 0 } or { Hour: 15, Minute: 0 })
                            {
                                DrawText($"{hourAndMinute.Hour}:{hourAndMinute.Minute:00}", position.Left);
                            }

                            position = position.ShiftLeft();
                            if (position.Left < 0)
                            {
                                break;
                            }
                        }

                        break;
                    case CandleInterval.FiveMinutes:
                        for (var i = 1; i < candles.Count; i++)
                        {
                            if (TradingDay.IsRegularHours(candles[i].Time) &&
                                BeginHourCandle(i) is { } hourAndMinute)
                            {
                                DrawText($"{hourAndMinute.Hour}:{hourAndMinute.Minute:00}", position.Left);
                            }

                            position = position.ShiftLeft();
                            if (position.Left < 0)
                            {
                                break;
                            }
                        }

                        break;
                    case CandleInterval.Minute:
                        for (var i = 1; i < candles.Count; i++)
                        {
                            var hourAndMinute = HourAndMinute.EndOfThirtyMinutesCandle(candles[i].Time);
                            if (hourAndMinute.Hour != 20 &&
                                hourAndMinute != HourAndMinute.EndOfThirtyMinutesCandle(candles[i - 1].Time))
                            {
                                DrawText($"{hourAndMinute.Hour}:{hourAndMinute.Minute:00}", position.Left);
                            }

                            position = position.ShiftLeft();
                            if (position.Left < 0)
                            {
                                break;
                            }
                        }

                        break;
                    default:
                        throw new InvalidEnumArgumentException();
                }

                HourAndMinute? BeginHourCandle(int index)
                {
                    var end = HourAndMinute.EndOfHourCandle(candles[index].Time);
                    if (index < candles.Count - 1 &&
                        end != HourAndMinute.EndOfHourCandle(candles[index + 1].Time))
                    {
                        return end switch
                        {
                            { Hour: 10 } => new HourAndMinute(09, 30),
                            { Hour: 9, Minute: 30 } => new HourAndMinute(09, 00),
                            _ => new HourAndMinute(end.Hour - 1, 00),
                        };
                    }

                    return null;
                }

                void DrawText(string text, double x)
                {
                    drawingContext.DrawText(
                        text,
                        typeface,
                        fontSize,
                        new Point(x, 0),
                        fill,
                        VisualTreeHelper.GetDpi(this));
                }
            }
        }
    }
}
