namespace Sideways
{
    internal static class Interpolate
    {
        internal static double Map(DoubleRange source, double value, DoubleRange target)
        {
            var m = source.Interpolate(value);
            return target.Min + (m * (target.Max - target.Min));
        }

        internal static float Map(DoubleRange source, double value, FloatRange target)
        {
            var m = (float)source.Interpolate(value);
            return target.Min + (m * (target.Max - target.Min));
        }

        internal static double Map(FloatRange source, float value, DoubleRange target)
        {
            var m = source.Interpolate(value);
            return target.Min + (m * (target.Max - target.Min));
        }
    }
}
