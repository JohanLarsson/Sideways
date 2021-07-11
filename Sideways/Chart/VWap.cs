﻿namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Media;

    public class VWap : CandlesElement
    {
        public static readonly DependencyProperty PriceRangeProperty = Chart.PriceRangeProperty.AddOwner(
            typeof(VWap),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty PriceScaleProperty = Chart.PriceScaleProperty.AddOwner(
            typeof(VWap),
            new FrameworkPropertyMetadata(
                Scale.Logarithmic,
                FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
            nameof(Stroke),
            typeof(SolidColorBrush),
            typeof(VWap),
            new FrameworkPropertyMetadata(
                default(SolidColorBrush),
                FrameworkPropertyMetadataOptions.AffectsRender,
                (d, e) => ((VWap)d).pen = null));

        private readonly DrawingVisual drawing;

        private Pen? pen;

        public VWap()
        {
            this.drawing = new DrawingVisual();
            this.AddVisualChild(this.drawing);
        }

        public FloatRange? PriceRange
        {
            get => (FloatRange?)this.GetValue(PriceRangeProperty);
            set => this.SetValue(PriceRangeProperty, value);
        }

        public Scale PriceScale
        {
            get => (Scale)this.GetValue(PriceScaleProperty);
            set => this.SetValue(PriceScaleProperty, value);
        }

        public SolidColorBrush? Stroke
        {
            get => (SolidColorBrush?)this.GetValue(StrokeProperty);
            set => this.SetValue(StrokeProperty, value);
        }

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index) => index == 0
            ? this.drawing
            : throw new ArgumentOutOfRangeException(nameof(index));

        protected override void OnRender(DrawingContext drawingContext)
        {
            using var context = this.drawing.RenderOpen();
            if (this.Stroke is { } stroke &&
                this.ItemsSource is { } candles &&
                this.PriceRange is { } priceRange)
            {
                this.pen ??= Brushes.CreatePen(stroke);
                Point? previous = null;
                var position = CandlePosition.RightToLeft(this.RenderSize, this.CandleWidth, new ValueRange(priceRange, this.PriceScale));
                foreach (var a in candles.DescendingVWaps(this.Time, this.CandleInterval))
                {
                    var p2 = new Point(position.Center, position.Y(a));
                    if (previous is { } p1)
                    {
                        context.DrawLine(
                            this.pen,
                            p1,
                            p2);
                    }

                    previous = p2;
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
