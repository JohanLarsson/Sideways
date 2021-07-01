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
            if (index > 0)
            {
                var changesBuilder = ImmutableArray.CreateBuilder<Percent>(Math.Min(index, 8));
                var previousBuilder = ImmutableArray.CreateBuilder<QuarterlyEarning>(index + 1);
                for (var i = index; i >= 0; i--)
                {
                    if (i > 0 &&
                        changesBuilder.Count < changesBuilder.Capacity)
                    {
                        changesBuilder.Add(Percent.Change(earnings[i - 1].ReportedEps, earnings[i].ReportedEps));
                    }

                    previousBuilder.Add(this.earnings[i]);
                }

                this.DescendingEarnings = previousBuilder.MoveToImmutable();
                this.DescendingChanges = changesBuilder.MoveToImmutable();
            }
        }

        public double Eps => this.earnings[this.index].ReportedEps;

        public DateTimeOffset Date => this.earnings[this.index].ReportedDate;

        public float? Estimate => this.earnings[this.index].EstimatedEps;

        public float? Surprise => this.earnings[this.index].Surprise;

        public Percent? SurprisePercentage => this.earnings[this.index].SurprisePercentage;

        public Percent? QoQ => this.index < 1
            ? null
            : Percent.Change(this.earnings[this.index - 1].ReportedEps, this.earnings[this.index].ReportedEps);

        public Percent? YoY => this.index < 4
            ? null
            : Percent.Change(this.earnings[this.index - 4].ReportedEps, this.earnings[this.index].ReportedEps);

        public ImmutableArray<QuarterlyEarning> DescendingEarnings { get; }

        public ImmutableArray<Percent> DescendingChanges { get; }
    }
}
