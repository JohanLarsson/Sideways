namespace Sideways
{
    using System;
    using System.Collections.Immutable;

    public class EarningsViewModel
    {
        private readonly ImmutableArray<QuarterlyEarning> earnings;
        private readonly int index;

        public EarningsViewModel(ImmutableArray<QuarterlyEarning> earnings, int index)
        {
            this.earnings = earnings;
            this.index = index;
        }

        public double Eps => this.earnings[this.index].ReportedEps;

        public DateTimeOffset Date => this.earnings[this.index].ReportedDate;

        public float? Surprise => this.earnings[this.index].SurprisePercentage;
    }
}
