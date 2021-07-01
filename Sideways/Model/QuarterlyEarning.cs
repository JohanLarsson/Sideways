namespace Sideways
{
    using System;
    using System.Globalization;

    public readonly struct QuarterlyEarning : IEquatable<QuarterlyEarning>
    {
        public QuarterlyEarning(DateTimeOffset fiscalDateEnding, DateTimeOffset reportedDate, float reportedEps, float? estimatedEps)
        {
            this.FiscalDateEnding = fiscalDateEnding;
            this.ReportedDate = reportedDate;
            this.ReportedEps = reportedEps;
            this.EstimatedEps = estimatedEps;
        }

        public DateTimeOffset FiscalDateEnding { get; }

        public DateTimeOffset ReportedDate { get; }

        public float ReportedEps { get; }

        public float? EstimatedEps { get; }

        public float? Surprise => this.ReportedEps - this.EstimatedEps;

        public Percent? SurprisePercentage => this.EstimatedEps is { } estimatedEps
            ? Percent.Change(estimatedEps, this.ReportedEps)
            : null;

        public static bool operator ==(QuarterlyEarning left, QuarterlyEarning right) => left.Equals(right);

        public static bool operator !=(QuarterlyEarning left, QuarterlyEarning right) => !left.Equals(right);

        public bool Equals(QuarterlyEarning other) => this.FiscalDateEnding.Equals(other.FiscalDateEnding) &&
                                                               this.ReportedDate.Equals(other.ReportedDate) &&
                                                               this.ReportedEps.Equals(other.ReportedEps) &&
                                                               this.EstimatedEps.Equals(other.EstimatedEps);

        public override bool Equals(object? obj) => obj is QuarterlyEarning other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.FiscalDateEnding, this.ReportedDate, this.ReportedEps, this.EstimatedEps);

        public override string ToString() => $"{this.FiscalDateEnding:yyyy-MM-dd} {this.ReportedDate:yyyy-MM-dd} {this.ReportedEps} {this.EstimatedEps?.ToString(CultureInfo.InvariantCulture) ?? "no estimate"}";
    }
}
