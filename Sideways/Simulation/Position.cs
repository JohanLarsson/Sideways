namespace Sideways
{
    using System.Collections.Immutable;

    public class Position
    {
        public Position(string symbol, ImmutableList<Buy> buys)
        {
            this.Symbol = symbol;
            this.Buys = buys;
        }

        public string Symbol { get; }

        public ImmutableList<Buy> Buys { get; }
    }
}
