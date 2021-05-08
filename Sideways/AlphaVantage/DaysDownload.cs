namespace Sideways.AlphaVantage
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;

    public class DaysDownload : IDownload
    {
        public DaysDownload(string symbol, OutputSize outputSize, Task<ImmutableArray<AdjustedCandle>> task)
        {
            this.Symbol = symbol;
            this.OutputSize = outputSize;
            this.Task = task;
        }

        public string Symbol { get; }

        public OutputSize OutputSize { get; }

        public Task<ImmutableArray<AdjustedCandle>> Task { get; }

        public DateTimeOffset Start { get; } = DateTimeOffset.Now;
    }
}
