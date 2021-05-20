namespace Sideways
{
    using System;

    public readonly struct Listing : IEquatable<Listing>
    {
        public Listing(string symbol, string? name, string exchange, string assetType, DateTimeOffset ipoDate, DateTimeOffset? delistingDate)
        {
            this.Symbol = symbol;
            this.Name = name;
            this.Exchange = exchange;
            this.AssetType = assetType;
            this.IpoDate = ipoDate;
            this.DelistingDate = delistingDate;
        }

        public string Symbol { get; }

        public string? Name { get; }

        public string Exchange { get; }

        public string AssetType { get; }

        public DateTimeOffset IpoDate { get; }

        public DateTimeOffset? DelistingDate { get; }

        public static bool operator ==(Listing left, Listing right) => left.Equals(right);

        public static bool operator !=(Listing left, Listing right) => !left.Equals(right);

        public bool Equals(Listing other) => this.Symbol == other.Symbol && this.Exchange == other.Exchange;

        public override bool Equals(object? obj) => obj is Listing other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.Symbol, this.Exchange);
    }
}
