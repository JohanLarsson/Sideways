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
            var candleWidth = this.CandleWidth;
            var candles = this.Candles;
            switch (this.CandleInterval)
            {
                case CandleInterval.Hour or CandleInterval.Minute:
                    for (var i = 0; i < Math.Min(candles.Count, Math.Ceiling(size.Width / candleWidth)); i++)
                    {
                        var candle = candles[i];
                        if (TradingDay.IsPreMarket(candle.Time))
                        {
                            var p1 = new Point(X(i), 0);
                            Skip(c => TradingDay.IsPreMarket(c.Time));
                            drawingContext.DrawRectangle(
                                Brushes.PreMarket,
                                null,
                                new Rect(
                                    p1,
                                    new Point(X(i + 1), size.Height)));
                        }
                        else if (TradingDay.IsPostMarket(candle.Time))
                        {
                            var p1 = new Point(X(i), 0);
                            Skip(c => TradingDay.IsPostMarket(c.Time));

                            drawingContext.DrawRectangle(
                                Brushes.PostMarket,
                                null,
                                new Rect(
                                    p1,
                                    new Point(X(i + 1), size.Height)));
                        }

                        void Skip(Func<Candle, bool> selector)
                        {
                            i++;
                            while (i < candles.Count - 1 &&
                                   selector(candles[i + 1]))
                            {
                                i++;
                            }
                        }

                        double X(int index) => Math.Max(0, size.Width - (index * candleWidth));
                    }

                    break;
                case CandleInterval.Day:
                    DrawEven(x => x.Time.Month);
                    break;
                case CandleInterval.Week:
                    DrawEven(x => x.Time.Year);
                    break;
            }

            void DrawEven(Func<Candle, int> func)
            {
                for (var i = 0; i < Math.Min(candles.Count, Math.Ceiling(size.Width / candleWidth)); i++)
                {
                    if (func(candles[i]) % 2 == 0)
                    {
                        var p1 = new Point(X(i), 0);
                        i++;
                        while (i < candles.Count - 1 &&
                               func(candles[i]) % 2 == 0)
                        {
                            i++;
                        }

                        drawingContext.DrawRectangle(
                            Brushes.Even,
                            null,
                            new Rect(
                                p1,
                                new Point(X(i + 1), size.Height)));
                    }

                    double X(int index) => Math.Max(0, size.Width - (index * candleWidth));
                }
            }
        }
    }
}
