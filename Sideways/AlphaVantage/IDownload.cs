namespace Sideways.AlphaVantage
{
    using System;

    public interface IDownload
    {
        public string Symbol { get; }

        public DateTimeOffset? Start { get; }

#pragma warning disable CA1716 // Identifiers should not match keywords
        DateTimeOffset? End { get; }
#pragma warning restore CA1716 // Identifiers should not match keywords

        Exception? Exception { get; }
    }
}
