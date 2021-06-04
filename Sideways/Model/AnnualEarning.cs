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

        public static bool operator ==(AnnualEarning left, AnnualEarning right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AnnualEarning left, AnnualEarning right)
        {
            return !left.Equals(right);
        }

        public bool Equals(AnnualEarning other)
        {
            return this.FiscalDateEnding.Equals(other.FiscalDateEnding) && this.ReportedEPS.Equals(other.ReportedEPS);
        }

        public override bool Equals(object? obj)
        {
            return obj is AnnualEarning other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.FiscalDateEnding, this.ReportedEPS);
        }
    }
}
