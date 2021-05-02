namespace Sideways
{
    internal static class Interpolate
    {
        internal static double Value(DoubleRange source, double value, DoubleRange target)
        {
            var m = source.Interpolate(value);
            return target.Min + (m * (target.Max - target.Min));
        }
    }
}
