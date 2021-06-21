namespace Sideways
{
    public class Measurement
    {
        public Measurement(TimeAndPrice start, TimeAndPrice? end)
        {
            this.Start = start;
            this.End = end;
        }

        public TimeAndPrice Start { get; }

        public TimeAndPrice? End { get; }
    }
}
