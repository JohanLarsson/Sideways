namespace Sideways
{
    using System.Windows;

    public abstract class AbstractBarChart : FrameworkElement
    {
        public static readonly DependencyProperty BarWidthProperty = DependencyProperty.Register(
            nameof(BarWidth),
            typeof(int),
            typeof(AbstractBarChart),
            new FrameworkPropertyMetadata(
                5,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty BarsProperty = DependencyProperty.Register(
            nameof(Bars),
            typeof(int),
            typeof(AbstractBarChart),
            new FrameworkPropertyMetadata(
                16,
                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender));

        public int BarWidth
        {
            get => (int)this.GetValue(BarWidthProperty);
            set => this.SetValue(BarWidthProperty, value);
        }

        public int Bars
        {
            get => (int)this.GetValue(BarsProperty);
            set => this.SetValue(BarsProperty, value);
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return new(this.Bars * this.BarWidth, 0);
        }
    }
}
