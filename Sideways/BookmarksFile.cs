namespace Sideways
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text.Json;
    using System.Windows;

    using Microsoft.Win32;

    public sealed class BookmarksFile : INotifyPropertyChanged
    {
        public static readonly string Directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Bookmarks");

        private static readonly JsonSerializerOptions JsonSerializerOptions = new() { WriteIndented = true };

        private FileInfo? file;

        private BookmarksFile(FileInfo? file, ImmutableSortedSet<Bookmark> bookmarks)
        {
            this.file = file;
            this.Bookmarks = new(bookmarks);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? FileName => this.file?.FullName;

        public string Name => this.file is { FullName: { } fullName } ? Path.GetFileNameWithoutExtension(fullName) : "Not saved";

        public ObservableSortedSet<Bookmark> Bookmarks { get; }

        public static BookmarksFile Create(FileInfo? file, IEnumerable<Bookmark> bookmarks) =>
            new(file, ImmutableSortedSet.CreateRange(BookMarkComparer.Default, bookmarks));

        public bool Add(Bookmark bookmark) => this.Bookmarks.Add(bookmark);

        public void Save()
        {
            if (!System.IO.Directory.Exists(Directory))
            {
                System.IO.Directory.CreateDirectory(Directory);
            }

            if (File() is { FullName: { } fileName })
            {
                System.IO.File.WriteAllText(fileName, JsonSerializer.Serialize(this.Bookmarks, JsonSerializerOptions));
            }

            FileInfo? File()
            {
                if (this.file is { } file)
                {
                    return file;
                }

                var dialog = new SaveFileDialog
                {
                    InitialDirectory = Directory,
                    FileName = $"{DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
                    DefaultExt = ".bookmarks",
                    Filter = "Bookmark files|*.bookmarks",
                };

                if (dialog.ShowDialog() is true)
                {
                    this.file = new FileInfo(dialog.FileName);
                    this.OnPropertyChanged(nameof(this.Name));
                    return new(dialog.FileName);
                }

                return null;
            }
        }

        public bool IsDirty()
        {
            if (this.file is null)
            {
                return true;
            }

            return this.file is { FullName: { } fileName } &&
                   JsonSerializer.Serialize(this.Bookmarks, JsonSerializerOptions) != File.ReadAllText(fileName);
        }

        public void AskSave()
        {
            if (this.IsDirty() &&
                MessageBox.Show("Do you want to save bookmarks?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.Save();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private sealed class BookMarkComparer : IComparer<Bookmark>
        {
            internal static readonly BookMarkComparer Default = new();

            public int Compare(Bookmark? x, Bookmark? y)
            {
                if (ReferenceEquals(x, y))
                {
                    return 0;
                }

                if (y is null)
                {
                    return 1;
                }

                if (x is null)
                {
                    return -1;
                }

                var state = string.Compare(x.Symbol, y.Symbol, StringComparison.Ordinal);
                if (state != 0)
                {
                    return state;
                }

                state = x.Time.CompareTo(y.Time);
                if (state != 0)
                {
                    return state;
                }

                return string.Compare(x.Comment, y.Comment, StringComparison.Ordinal);
            }
        }
    }
}
