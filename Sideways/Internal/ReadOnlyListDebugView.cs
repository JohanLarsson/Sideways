#pragma warning disable GU0073 // Member of non-public type should be internal.
namespace Sideways
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal sealed class ReadOnlyListDebugView<T>
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
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
