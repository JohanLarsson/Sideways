namespace Sideways
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class ReadOnlyListDebugView<T>
    {
        private readonly IReadOnlyList<T> collection;

        public ReadOnlyListDebugView(IReadOnlyList<T> collection)
        {
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items => this.collection.ToArray();
    }
}
