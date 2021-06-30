namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;

    public sealed class BookmarksViewModel : INotifyPropertyChanged
    {
        private ImmutableList<BookmarksFile> bookmarkFiles = ImmutableList<BookmarksFile>.Empty;
        private BookmarksFile? selectedBookmarkFile;
        private Bookmark? selectedBookmark;
        private int offset;

        public BookmarksViewModel()
        {
            this.NewCommand = new RelayCommand(_ => this.Add(BookmarksFile.Create(null, ImmutableList<Bookmark>.Empty)));
            this.SaveCommand = new RelayCommand(_ => this.selectedBookmarkFile?.Save(), _ => this.selectedBookmarkFile is { });
            this.CloseCommand = new RelayCommand(
                _ =>
                {
                    if (this.selectedBookmarkFile is { } selected)
                    {
                        selected.AskSave();
                        this.Remove(selected);
                    }
                },
                _ => this.selectedBookmarkFile is { });
            this.ScanCommand = new RelayCommand(_ => RunScan());

            async void RunScan()
            {
                try
                {
                    var bookmarks = await Task.Run(() => this.ScanViewModel.Run().ToArray()).ConfigureAwait(false);
                    this.Add(BookmarksFile.Create(null, bookmarks));
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    _ = MessageBox.Show(e.Message, "Scan", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand NewCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand CloseCommand { get; }

        public ICommand ScanCommand { get; }

        public Scan.ScanViewModel ScanViewModel { get; } = new();

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
