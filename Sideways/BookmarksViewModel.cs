namespace Sideways
{
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    using Sideways.AlphaVantage;

    public sealed class BookmarksViewModel : INotifyPropertyChanged
    {
        private ImmutableList<BookmarksFile> bookmarkFiles = ImmutableList<BookmarksFile>.Empty;
        private BookmarksFile? selectedBookmarkFile;
        private Bookmark? selectedBookmark;
        private int offset;

        public BookmarksViewModel(Downloader downloader)
        {
            this.Downloader = downloader;
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

            this.DownloadAllCommand = new RelayCommand(
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _ => downloader.DownloadAllAsync(d => this.selectedBookmarkFile?.Bookmarks.Any(x => x.Symbol == d.Symbol) is true),
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _ => this.selectedBookmarkFile is { Bookmarks: { Count: > 0 } } &&
                     downloader is { SymbolDownloads: { IsEmpty: false }, SymbolDownloadState: { Status: DownloadStatus.Waiting or DownloadStatus.Completed or DownloadStatus.Error } });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public Downloader Downloader { get; }

        public ICommand NewCommand { get; }

        public ICommand SaveCommand { get; }

        public ICommand CloseCommand { get; }

        public ICommand DownloadAllCommand { get; }

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
