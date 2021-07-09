namespace Sideways
{
    using System.Windows.Media;

    public static class Brushes
    {
        public static readonly SolidColorBrush Background = Create(Color.FromArgb(byte.MaxValue, 5, 5, 15));
        public static readonly SolidColorBrush SemiTransparentBackground = Create(Color.FromArgb(100, 5, 5, 15));
        public static readonly SolidColorBrush MeasureBackground = Create(Color.FromArgb(30, 160, 160, 160));
        public static readonly SolidColorBrush Increasing = Create(Color.FromArgb(byte.MaxValue, 66, 154, 66));
        public static readonly SolidColorBrush Decreasing = Create(Color.FromArgb(byte.MaxValue, 154, 66, 66));
        public static readonly SolidColorBrush PreMarket = Create(Color.FromArgb(9, 255, 255, 20));
        public static readonly SolidColorBrush PostMarket = Create(Color.FromArgb(9, 50, 50, 255));
        public static readonly SolidColorBrush Even = Create(Color.FromArgb(10, 80, 80, 80));

        public static readonly SolidColorBrush Ma10 = Create(Color.FromArgb(150, 99, 61, 143));
        public static readonly SolidColorBrush Ma20 = Create(Color.FromArgb(120, 143, 139, 61));
        public static readonly SolidColorBrush Ma50 = Create(Color.FromArgb(80, 143, 61, 61));
        public static readonly SolidColorBrush Ma150 = Create(Color.FromArgb(80, 95, 158, 160));
        public static readonly SolidColorBrush Ma100 = Create(Color.FromArgb(80, 143, 113, 61));
        public static readonly SolidColorBrush Ma200 = Create(Color.FromArgb(80, 61, 143, 61));

        public static readonly SolidColorBrush LightGray = Create(Color.FromArgb(byte.MaxValue, 160, 160, 160));
        public static readonly SolidColorBrush Gray = Create(Color.FromArgb(byte.MaxValue, 128, 128, 128));
        public static readonly SolidColorBrush MediumDarkGray = Create(Color.FromArgb(byte.MaxValue, 80, 80, 80));
        public static readonly SolidColorBrush DarkGray = Create(Color.FromArgb(byte.MaxValue, 40, 40, 40));
        public static readonly SolidColorBrush CrossHair = Create(Color.FromArgb(160, byte.MaxValue, byte.MaxValue, byte.MaxValue));
        public static readonly SolidColorBrush BookMark = Create(Color.FromArgb(80, 90, 140, byte.MaxValue));
        public static readonly SolidColorBrush SelectedBookMark = Create(Color.FromArgb(byte.MaxValue, 110, 160, byte.MaxValue));

        public static readonly SolidColorBrush Transparent = System.Windows.Media.Brushes.Transparent;
        public static readonly SolidColorBrush Accent = Create(Color.FromArgb(byte.MaxValue, 100, 149, 237));
        public static readonly SolidColorBrush Error = Create(Color.FromArgb(byte.MaxValue, 205, 92, 92));
        public static readonly SolidColorBrush Pressed = Accent;
        public static readonly SolidColorBrush DisabledText = Create(Color.FromArgb(byte.MaxValue, 128, 128, 128));
        public static readonly SolidColorBrush Text = DisabledText;
        public static readonly SolidColorBrush EnabledText = Create(Color.FromArgb(byte.MaxValue, 180, 180, 180));
        public static readonly SolidColorBrush SelectedText = EnabledText;
        public static readonly SolidColorBrush SelectedBackground = DarkGray;
        public static readonly Color SelectedBackgroundColor = SelectedBackground.Color;

        public static SolidColorBrush Get(Candle candle) => candle.Open < candle.Close ? Increasing : Decreasing;

        public static Pen CreatePen(SolidColorBrush brush)
        {
            var pen = new Pen(brush, 1);
            pen.Freeze();
            return pen;
        }

        private static SolidColorBrush Create(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
    }
}
