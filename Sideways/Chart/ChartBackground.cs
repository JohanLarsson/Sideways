namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class ChartBackground : CandleSeries
    {
        protected override void OnRender(DrawingContext drawingContext)
        {
            var size = this.RenderSize;
            var candles = this.Candles;
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
                var position = CandlePosition.Create(this.RenderSize, this.CandleWidth, default);
                for (var i = 0; i < candles.Count; i++)
                {
                    if (func(candles[i]))
                    {
                        var p1 = new Point(position.Right, size.Height);

                        while (i < candles.Count - 1 &&
                               func(candles[i]))
                        {
                            i++;
                            position = position.ShiftLeft();
                            if (position.Right < 0)
                            {
                                break;
                            }
                        }

                        drawingContext.DrawRectangle(
                            brush,
                            null,
                            new Rect(
                                p1,
                                new Point(position.Left, 0)));
                    }

                    position = position.ShiftLeft();
                    if (position.Right < 0)
                    {
                        break;
                    }
                }
            }
        }
    }
}
