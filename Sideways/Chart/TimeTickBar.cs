﻿namespace Sideways
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
            if (this.Candles is { Count: > 10 } candles)
            {
                var typeface = new Typeface(
                    TextElement.GetFontFamily(this),
                    TextElement.GetFontStyle(this),
                    TextElement.GetFontWeight(this),
                    TextElement.GetFontStretch(this));
                var fontSize = TextElement.GetFontSize(this);
                var fill = this.Fill;
                var position = CandlePosition.Create(this.RenderSize, this.CandleWidth, default);

                switch (this.CandleInterval)
                {
                    case CandleInterval.Week:
                        for (var i = 0; i < this.Candles.Count - 1; i++)
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
                        for (var i = 0; i < this.Candles.Count - 1; i++)
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
                        foreach (var candle in this.Candles)
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
                    case CandleInterval.Minute:
                        foreach (var candle in this.Candles)
                        {
                            switch (candle.Time)
                            {
                                case { Hour: 9, Minute: 31 }:
                                    DrawText("09:30", position.Left);
                                    break;
                                case { Hour: 12 }:
                                    DrawText("12:00", position.Left);
                                    break;
                                case { Hour: 15 }:
                                    DrawText("15:00", position.Left);
                                    break;
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
