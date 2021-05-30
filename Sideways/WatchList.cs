namespace Sideways
{
    using System.Collections.ObjectModel;

    public class WatchList
    {
        public ObservableCollection<string> Symbols { get; } = new();
    }
}
