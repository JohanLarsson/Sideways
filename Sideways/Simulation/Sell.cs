namespace Sideways
{
    using System;

    public class Sell
    {
        public Sell(int shares, DateTimeOffset time, float price)
        {
            this.Shares = shares;
            this.Time = time;
            this.Price = price;
        }

        public int Shares { get; }

        public DateTimeOffset Time { get; }

        public float Price { get; }
    }
}
