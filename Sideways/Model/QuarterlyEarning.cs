namespace Sideways
{
    using System;

    public readonly struct QuarterlyEarning
    {
        public QuarterlyEarning(DateTimeOffset fiscalDateEnding, DateTimeOffset reportedDate, float reportedEps, float estimatedEps)
        {
            this.FiscalDateEnding = fiscalDateEnding;
            this.ReportedDate = reportedDate;
            this.ReportedEps = reportedEps;
            this.EstimatedEps = estimatedEps;
        }

        public DateTimeOffset FiscalDateEnding { get; }

        public DateTimeOffset ReportedDate { get; }

        public float ReportedEps { get; }

        public float EstimatedEps { get; }

        public float Surprise => this.ReportedEps - this.EstimatedEps;

        public float SurprisePercentage => this.Surprise / this.EstimatedEps;
    }
}
