namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class ChartBackground : CandleSeries
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            switch (this.CandleInterval)
            {
                case CandleInterval.Hour or CandleInterval.Minute:
                    DrawBand(x => TradingDay.IsPreMarket(x.Time), Brushes.PreMarket);
                    DrawBand(x => TradingDay.IsPostMarket(x.Time), Brushes.PostMarket);
                    break;
                case CandleInterval.Day:
                    DrawBand(x => x.Time.Month % 2 == 0, Brushes.Even);
                    break;
                case CandleInterval.Week:
                    DrawBand(x => x.Time.Year % 2 == 0, Brushes.Even);
                    break;
            }

            void DrawBand(Func<Candle, bool> func, SolidColorBrush brush)
            {
                var renderSize = this.RenderSize;
                var candles = this.Candles;
                var position = CandlePosition.Create(renderSize, this.CandleWidth, default);
                for (var i = 0; i < candles.Count; i++)
                {
                    if (func(candles[i]))
                    {
                        var p2 = new Point(position.Right, renderSize.Height);

                        while (i < candles.Count - 1 &&
                               func(candles[i]))
                        {
                            i++;
                            position = position.ShiftLeft();
                            if (position.Left < 0)
                            {
                                break;
                            }
                        }

                        drawingContext.DrawRectangle(
                            brush,
                            null,
                            new Rect(
                                new Point(position.Right, 0),
                                p2));
                    }

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
