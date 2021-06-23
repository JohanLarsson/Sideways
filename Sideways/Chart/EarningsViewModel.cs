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

        public float? Estimate => this.earnings[this.index].Surprise;

        public float? Surprise => this.earnings[this.index].Surprise;

        public float? SurprisePercentage => this.earnings[this.index].SurprisePercentage;

        public float? QoQ => this.index < 1
            ? null
            : PercentChange(this.earnings[this.index - 1].ReportedEps, this.earnings[this.index].ReportedEps);

        public float? YoY => this.index < 4
            ? null
            : PercentChange(this.earnings[this.index - 4].ReportedEps, this.earnings[this.index].ReportedEps);

        private static float PercentChange(float from, float to) => 100 * (to - from) / from;
    }
}
