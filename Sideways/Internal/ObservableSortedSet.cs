namespace Sideways
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class ObservableSortedSet<T> : IReadOnlyList<T>, ICollection<T>, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private ImmutableSortedSet<T> inner;

        public ObservableSortedSet(ImmutableSortedSet<T> inner)
        {
            this.inner = inner;
        }

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int Count => this.inner.Count;

        public bool IsReadOnly => false;

        public T this[int index] => this.inner[index];

        public IEnumerator<T> GetEnumerator() => this.inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public bool Add(T item)
        {
            var before = this.inner.Count;
            this.inner = this.inner.Add(item);
            this.OnPropertyChanged(nameof(this.Count));
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return this.inner.Count > before;
        }

        public void AddRange(IEnumerable<T> items)
        {
            this.inner = this.inner.Union(items);
            this.OnPropertyChanged(nameof(this.Count));
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        void ICollection<T>.Add(T item) => this.Add(item);

        public bool Remove(T item)
        {
            var before = this.inner.Count;
            this.inner = this.inner.Remove(item);
            this.OnPropertyChanged(nameof(this.Count));
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            return this.inner.Count > before;
        }

        public void Clear()
        {
            this.inner = this.inner.Clear();
            this.OnPropertyChanged(nameof(this.Count));
            this.OnPropertyChanged("Item[]");
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(T item) => this.inner.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)this.inner).CopyTo(array, arrayIndex);

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            this.CollectionChanged?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
