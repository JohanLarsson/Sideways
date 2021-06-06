namespace Sideways
{
    using System;

    public readonly struct AnnualEarning : IEquatable<AnnualEarning>
    {
        public AnnualEarning(DateTimeOffset fiscalDateEnding, float reportedEps)
        {
            this.FiscalDateEnding = fiscalDateEnding;
            this.ReportedEPS = reportedEps;
        }

        public DateTimeOffset FiscalDateEnding { get; }

        public float ReportedEPS { get; }

        public static bool operator ==(AnnualEarning left, AnnualEarning right) => left.Equals(right);

        public static bool operator !=(AnnualEarning left, AnnualEarning right) => !left.Equals(right);

        public bool Equals(AnnualEarning other) => this.FiscalDateEnding.Equals(other.FiscalDateEnding) && this.ReportedEPS.Equals(other.ReportedEPS);

        public override bool Equals(object? obj) => obj is AnnualEarning other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.FiscalDateEnding, this.ReportedEPS);

        public override string ToString() => $"{this.FiscalDateEnding:yyyy-MM-dd} {this.ReportedEPS}";
    }
}
