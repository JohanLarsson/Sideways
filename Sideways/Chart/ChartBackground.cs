namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class ChartBackground : CandleSeries
    {
        public static readonly DependencyProperty BookmarkTimeProperty = Chart.BookmarkTimeProperty.AddOwner(
            typeof(ChartBackground),
            new FrameworkPropertyMetadata(
                default(DateTimeOffset?),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public DateTimeOffset? BookmarkTime
        {
            get => (DateTimeOffset?)this.GetValue(BookmarkTimeProperty);
            set => this.SetValue(BookmarkTimeProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderSize = this.RenderSize;
            var candles = this.Candles;
            var candleWidth = this.CandleWidth;
            switch (this.CandleInterval)
            {
                case CandleInterval.Hour or CandleInterval.Minute:
                    DrawBetweenDays(Brushes.PreMarket);
                    DrawBand(x => TradingDay.IsPreMarket(x.Time), Brushes.PreMarket);
                    DrawBand(x => TradingDay.IsPostMarket(x.Time), Brushes.PostMarket);
                    break;

                    void DrawBetweenDays(SolidColorBrush brush)
                    {
                        var position = CandlePosition.Create(renderSize, candleWidth, default);
                        for (var i = 0; i < candles.Count - 1; i++)
                        {
                            if (candles[i].Time.Date != candles[i + 1].Time.Date &&
                                TradingDay.IsOrdinaryHours(candles[i].Time) &&
                                TradingDay.IsOrdinaryHours(candles[i + 1].Time))
                            {
                                drawingContext.DrawRectangle(
                                    brush,
                                    null,
                                    new Rect(
                                        new Point(position.Left, 0),
                                        new Point(position.Left + 1, renderSize.Height)));
                            }

                            position = position.ShiftLeft();
                            if (position.Left < 0)
                            {
                                break;
                            }
                        }
                    }

                case CandleInterval.Day:
                    DrawBand(x => x.Time.Month % 2 == 0, Brushes.Even);
                    break;
                case CandleInterval.Week:
                    DrawBand(x => x.Time.Year % 2 == 0, Brushes.Even);
                    break;
            }

            if (this.BookmarkTime is { } bookmarkTime &&
                CandlePosition.X(bookmarkTime, candles, renderSize.Width, candleWidth, this.CandleInterval) is { } bookMarkX)
            {
                drawingContext.DrawRectangle(
                    Brushes.DarkGray,
                    null,
                    new Rect(
                        new Point(bookMarkX, 0),
                        new Point(bookMarkX + 1, renderSize.Height)));
            }

            void DrawBand(Func<Candle, bool> func, SolidColorBrush brush)
            {
                var position = CandlePosition.Create(renderSize, candleWidth, default);
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
