namespace Sideways.Scan
{
    using System;

    public class HasMinutesCriteria : Criteria
    {
        public override string Info => "Has minutes";

        public override bool IsSatisfied(SortedCandles candles, int index)
        {
            throw new InvalidOperationException("Used when reading from db.");
        }
    }
}
