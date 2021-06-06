namespace Sideways
{
    using System;
    using System.Collections.Immutable;

    public class Bookmark
    {
        public Bookmark(string symbol, DateTimeOffset time, ImmutableSortedSet<string> tags, string? comment)
        {
            this.Symbol = symbol;
            this.Time = time;
            this.Tags = tags;
            this.Comment = comment;
        }

        public string Symbol { get; }

        public DateTimeOffset Time { get; }

        public ImmutableSortedSet<string> Tags { get; }

        public string? Comment { get; }
    }
}
