namespace Sideways
{
    using System.Windows.Media;

    public static class Brushes
    {
        public static readonly SolidColorBrush Increasing = Create(Color.FromArgb(byte.MaxValue, 50, 170, 50));
        public static readonly SolidColorBrush Decreasing = Create(Color.FromArgb(byte.MaxValue, 170, 50, 50));
        public static readonly SolidColorBrush PreMarket = Create(Color.FromArgb(10, 255, 255, 20));
        public static readonly SolidColorBrush PostMarket = Create(Color.FromArgb(10, 50, 50, 255));
        public static readonly SolidColorBrush Even = Create(Color.FromArgb(10, 80, 80, 80));
        public static readonly SolidColorBrush Gray = System.Windows.Media.Brushes.Gray;

        public static SolidColorBrush Get(Candle candle) => candle.Open < candle.Close ? Increasing : Decreasing;

        private static SolidColorBrush Create(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
