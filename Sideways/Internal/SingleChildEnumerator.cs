namespace Sideways
{
    using System;
    using System.Collections;

    internal class SingleChildEnumerator : IEnumerator
    {
        private readonly object? child;
        private int index = -1;

        internal SingleChildEnumerator(object? child)
        {
            this.child = child;
        }

        object? IEnumerator.Current => this.index == 0 ? this.child : throw new InvalidOperationException("Invalid index.");

        bool IEnumerator.MoveNext()
        {
            this.index++;
            return this.index == 0;
        }

        void IEnumerator.Reset() => this.index = -1;
    }
}
