namespace Sideways
{
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public sealed class BookmarksViewModel : INotifyPropertyChanged
    {
        private ImmutableList<BookmarksFile> bookmarkFiles = ImmutableList<BookmarksFile>.Empty;
        private BookmarksFile? selectedBookmarkFile;
        private Bookmark? selectedBookmark;
        private int offset;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ImmutableList<BookmarksFile> BookmarkFiles
        {
            get => this.bookmarkFiles;
            private set
            {
                if (ReferenceEquals(value, this.bookmarkFiles))
                {
                    return;
                }

                this.bookmarkFiles = value;
                this.OnPropertyChanged();
            }
        }

        public BookmarksFile? SelectedBookmarkFile
        {
            get => this.selectedBookmarkFile;
            set
            {
                if (ReferenceEquals(value, this.selectedBookmarkFile))
                {
                    return;
                }

                this.selectedBookmarkFile = value;
                this.OnPropertyChanged();
            }
        }

        public Bookmark? SelectedBookmark
        {
            get => this.selectedBookmark;
            set
            {
                if (ReferenceEquals(value, this.selectedBookmark))
                {
                    return;
                }

                this.selectedBookmark = value;
                this.OnPropertyChanged();
            }
        }

        public int Offset
        {
            get => this.offset;
            set
            {
                if (value == this.offset)
                {
                    return;
                }

                this.offset = value;
                this.OnPropertyChanged();
            }
        }

        public void Remove(BookmarksFile bookmarksFile)
        {
            this.BookmarkFiles = this.bookmarkFiles.Remove(bookmarksFile);
            if (ReferenceEquals(this.selectedBookmarkFile, bookmarksFile))
            {
                this.SelectedBookmarkFile = null;
            }
        }

        public void Add(BookmarksFile bookmarksFile)
        {
            this.BookmarkFiles = this.bookmarkFiles.Add(bookmarksFile);
            this.SelectedBookmarkFile = bookmarksFile;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
