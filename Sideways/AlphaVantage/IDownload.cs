namespace Sideways.AlphaVantage
{
    using System;

    public interface IDownload
    {
        public string Symbol { get; }

        public DateTimeOffset? Start { get; }

        DateTimeOffset? End { get; }

        Exception? Exception { get; }
    }
}
