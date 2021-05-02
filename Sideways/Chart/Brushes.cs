namespace Sideways
{
    using System.Windows.Media;

    public static class Brushes
    {
        public static readonly SolidColorBrush Increasing = Create(Color.FromArgb(byte.MaxValue, 50, 170, 50));
        public static readonly SolidColorBrush Decreasing = Create(Color.FromArgb(byte.MaxValue, 170, 50, 50));

        public static SolidColorBrush Get(Candle candle) => candle.Open < candle.Close ? Increasing : Decreasing;

        private static SolidColorBrush Create(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
