namespace Sideways
{
    using System;
    using System.Collections;

    internal sealed class EmptyEnumerator : IEnumerator
    {
        internal static readonly IEnumerator Instance = new EmptyEnumerator();

        private EmptyEnumerator()
        {
        }

#pragma warning disable 6503 // "Property get methods should not throw exceptions."
        object IEnumerator.Current => throw new InvalidOperationException();
#pragma warning restore 6503

        public void Reset()
        {
        }

        public bool MoveNext() => false;
    }
}
