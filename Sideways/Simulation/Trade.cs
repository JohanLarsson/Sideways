namespace Sideways
{
    using System.Collections.Immutable;

    public class Trade
    {
        public Trade(string symbol, ImmutableList<Buy> buys, Sell sell)
        {
            this.Symbol = symbol;
            this.Buys = buys;
            this.Sell = sell;
        }

        public string Symbol { get; }

        public ImmutableList<Buy> Buys { get; }

        public Sell Sell { get; }
    }
}
