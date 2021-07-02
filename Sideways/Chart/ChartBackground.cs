namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Windows;
    using System.Windows.Media;

    public class ChartBackground : CandleSeries
    {
        public static readonly DependencyProperty BookmarksProperty = DependencyProperty.Register(
            nameof(Bookmarks),
            typeof(ObservableSortedSet<Bookmark>),
            typeof(ChartBackground),
            new FrameworkPropertyMetadata(
                default(ObservableSortedSet<Bookmark>),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SelectedBookmarkProperty = DependencyProperty.Register(
            nameof(SelectedBookmark),
            typeof(Bookmark),
            typeof(ChartBackground),
            new FrameworkPropertyMetadata(
                default(Bookmark),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty SelectedScanResultProperty = DependencyProperty.Register(
            nameof(SelectedScanResult),
            typeof(Bookmark),
            typeof(ChartBackground),
            new FrameworkPropertyMetadata(
                default(Bookmark),
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty EarningsProperty = EarningsBar.EarningsProperty.AddOwner(
            typeof(ChartBackground),
            new FrameworkPropertyMetadata(
                default(ImmutableArray<QuarterlyEarning>),
                FrameworkPropertyMetadataOptions.AffectsRender));

        private Pen? bookmarkPen;
        private Pen? selectedBookmarkPen;
        private Pen? selectedScanResultPen;
        private Pen? earningPen;

        public ObservableSortedSet<Bookmark>? Bookmarks
        {
            get => (ObservableSortedSet<Bookmark>?)this.GetValue(BookmarksProperty);
            set => this.SetValue(BookmarksProperty, value);
        }

        public Bookmark? SelectedBookmark
        {
            get => (Bookmark?)this.GetValue(SelectedBookmarkProperty);
            set => this.SetValue(SelectedBookmarkProperty, value);
        }

        public Bookmark? SelectedScanResult
        {
            get => (Bookmark?)this.GetValue(SelectedScanResultProperty);
            set => this.SetValue(SelectedScanResultProperty, value);
        }

        public ImmutableArray<QuarterlyEarning> Earnings
        {
            get => (ImmutableArray<QuarterlyEarning>)this.GetValue(EarningsProperty);
            set => this.SetValue(EarningsProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            var renderSize = this.RenderSize;
            var candles = this.Candles;
            var candleWidth = this.CandleWidth;
            switch (this.CandleInterval)
            {
                case CandleInterval.Hour or CandleInterval.FifteenMinutes or CandleInterval.FiveMinutes or CandleInterval.Minute:
                    DrawBetweenDays(Brushes.PreMarket);
                    DrawBand(x => TradingDay.IsPreMarket(x.Time), Brushes.PreMarket);
                    DrawBand(x => TradingDay.IsPostMarket(x.Time), Brushes.PostMarket);
                    break;

                    void DrawBetweenDays(SolidColorBrush brush)
                    {
                        var position = CandlePosition.RightToLeft(renderSize, candleWidth, default);
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

            if (this.Earnings is { IsDefaultOrEmpty: false } earnings)
            {
                foreach (var earning in earnings)
                {
                    if (CandlePosition.X(earning.ReportedDate, candles, renderSize.Width, candleWidth, this.CandleInterval) is { } earningX)
                    {
                        drawingContext.DrawLine(
                            this.earningPen ??= CreatePen(Brushes.Gray, 0.25),
                            new Point(earningX, 0),
                            new Point(earningX, renderSize.Height));
                    }
                }
            }

            if (this.Bookmarks is { Count: > 0 } bookmarks)
            {
                foreach (var bookmark in bookmarks)
                {
                    DrawLine(bookmark.Time, this.bookmarkPen ??= CreatePen(Brushes.BookMark, 0.25));
                }
            }

            if (this.SelectedBookmark is { } selectedBookmark)
            {
                DrawLine(selectedBookmark.Time, this.selectedBookmarkPen ??= CreatePen(Brushes.SelectedBookMark, 0.25));
            }

            if (this.SelectedScanResult is { } selectedScanResult)
            {
                DrawLine(selectedScanResult.Time, this.selectedScanResultPen ??= CreatePen(Brushes.SelectedBookMark, 0.25, DashStyles.Dash));
            }

            void DrawBand(Func<Candle, bool> func, SolidColorBrush brush)
            {
                var position = CandlePosition.RightToLeft(renderSize, candleWidth, default);
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

            void DrawLine(DateTimeOffset time, Pen pen)
            {
                if (CandlePosition.X(time, candles, renderSize.Width, candleWidth, this.CandleInterval) is { } bookMarkX &&
                    bookMarkX < renderSize.Width - this.CandleWidth)
                {
                    drawingContext.DrawLine(
                        pen,
                        new Point(bookMarkX, 0),
                        new Point(bookMarkX, renderSize.Height));
                }
            }
        }

        private static Pen CreatePen(SolidColorBrush brush, double thickness, DashStyle? dashStyle = null)
        {
            var pen = new Pen(brush, thickness)
            {
                DashStyle = dashStyle,
            };
            pen.Freeze();
            return pen;
        }
    }
}
