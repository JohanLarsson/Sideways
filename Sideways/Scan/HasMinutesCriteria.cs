namespace Sideways.Scan
{
    using System;

    public class HasMinutesCriteria : Criteria
    {
        public override string Info => "Has minutes";

        public bool IsSatisfied(SortedCandles candles, int index, DateTimeOffset? firstMinute)
        {
            if (!this.IsActive)
            {
                return true;
            }

            if (firstMinute is null)
            {
                return false;
            }

            return candles[index].Time >= firstMinute;
        }
    }
}
