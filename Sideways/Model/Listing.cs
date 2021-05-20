namespace Sideways
{
    using System;

    public readonly struct Listing
    {
        public Listing(string symbol, string name, string exchange, string assetType, DateTimeOffset ipoDate, DateTimeOffset? delistingDate)
        {
            this.Symbol = symbol;
            this.Name = name;
            this.Exchange = exchange;
            this.AssetType = assetType;
            this.IpoDate = ipoDate;
            this.DelistingDate = delistingDate;
        }

        public string Symbol { get; }

        public string Name { get; }

        public string Exchange { get; }

        public string AssetType { get; }

        public DateTimeOffset IpoDate { get; }

        public DateTimeOffset? DelistingDate { get; }
    }
}
