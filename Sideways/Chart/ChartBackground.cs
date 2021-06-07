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
            var position = CandlePosition.Create(renderSize, this.CandleWidth, default);

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

            if (this.BookmarkTime is { } bookmarkTime &&
                position.X(bookmarkTime, this.Candles) is { } bookMarkX)
            {
                drawingContext.DrawRectangle(
                    Brushes.Gray,
                    null,
                    new Rect(
                        new Point(bookMarkX, 0),
                        new Point(bookMarkX + 1, renderSize.Height)));
            }

            void DrawBand(Func<Candle, bool> func, SolidColorBrush brush)
            {
                var candles = this.Candles;
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
