namespace Sideways.AlphaVantage
{
    using System.Collections.Immutable;

    public class Earnings
    {
        public Earnings(string symbol, ImmutableArray<AnnualEarning> annualEarnings, ImmutableArray<QuarterlyEarning> quarterlyEarnings)
        {
            this.Symbol = symbol;
            this.AnnualEarnings = annualEarnings;
            this.QuarterlyEarnings = quarterlyEarnings;
        }

        public string Symbol { get; }

        public ImmutableArray<AnnualEarning> AnnualEarnings { get; }

        public ImmutableArray<QuarterlyEarning> QuarterlyEarnings { get; }
    }
}
