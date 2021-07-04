namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Microsoft.Win32;

    public partial class BookmarksView : UserControl
    {
        public BookmarksView()
        {
            this.InitializeComponent();
        }

        private void OnOpen(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is BookmarksViewModel viewModel)
            {
                if (!Directory.Exists(BookmarksFile.Directory))
                {
                    Directory.CreateDirectory(BookmarksFile.Directory);
                }

                var dialog = new OpenFileDialog
                {
                    InitialDirectory = BookmarksFile.Directory,
                    Filter = "Bookmark files|*.bookmarks",
                };

                if (dialog.ShowDialog() is true)
                {
                    if (viewModel.BookmarkFiles.Any(x => x.FileName == dialog.FileName))
                    {
                        _ = MessageBox.Show("Bookmark file already open.", "Bookmark", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }

                    try
                    {
                        viewModel.Add(
                            BookmarksFile.Create(
                                new FileInfo(dialog.FileName),
                                JsonSerializer.Deserialize<ImmutableList<Bookmark>>(File.ReadAllText(dialog.FileName)) ?? ImmutableList<Bookmark>.Empty));
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        _ = MessageBox.Show(exception.Message, "Invalid bookmark file.", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    e.Handled = true;
                }
            }
        }

        private void OnCanDelete(object sender, CanExecuteRoutedEventArgs e)
        {
            if (sender is DataGrid { ItemsSource: ObservableSortedSet<Bookmark>, SelectedItem: Bookmark })
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private void OnDelete(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is DataGrid { ItemsSource: ObservableSortedSet<Bookmark> bookmarks, SelectedItem: Bookmark item })
            {
                e.Handled = bookmarks.Remove(item);
            }
        }
    }
}
