namespace Sideways
{
    using System;

    public class Buy
    {
        public Buy(int shares, DateTimeOffset time, float price)
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
