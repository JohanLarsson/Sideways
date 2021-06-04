namespace Sideways
{
    using System;

    public readonly struct AnnualEarning
    {
        public AnnualEarning(DateTimeOffset fiscalDateEnding, float reportedEps)
        {
            this.FiscalDateEnding = fiscalDateEnding;
            this.ReportedEPS = reportedEps;
        }

        public DateTimeOffset FiscalDateEnding { get; }

        public float ReportedEPS { get; }
    }
}
