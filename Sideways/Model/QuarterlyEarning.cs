namespace Sideways
{
    using System;

    public readonly struct QuarterlyEarning : IEquatable<QuarterlyEarning>
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

        public static bool operator ==(QuarterlyEarning left, QuarterlyEarning right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(QuarterlyEarning left, QuarterlyEarning right)
        {
            return !left.Equals(right);
        }

        public bool Equals(QuarterlyEarning other)
        {
            return this.FiscalDateEnding.Equals(other.FiscalDateEnding) && this.ReportedDate.Equals(other.ReportedDate) && this.ReportedEps.Equals(other.ReportedEps) && this.EstimatedEps.Equals(other.EstimatedEps);
        }

        public override bool Equals(object? obj)
        {
            return obj is QuarterlyEarning other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.FiscalDateEnding, this.ReportedDate, this.ReportedEps, this.EstimatedEps);
        }
    }
}
