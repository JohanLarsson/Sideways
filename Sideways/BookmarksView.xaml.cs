namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.Globalization;
    using System.IO;
    using System.Text.Json;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Microsoft.Win32;

    public partial class BookmarksView : UserControl
    {
        private static readonly string BookmarksDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Bookmarks");

        public BookmarksView()
        {
            this.InitializeComponent();
        }

        private static MessageBoxResult ShowMessageBox(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage messageBoxImage = MessageBoxImage.None)
        {
            if (Application.Current.MainWindow is { } window)
            {
                return MessageBox.Show(window, messageBoxText, caption, button, messageBoxImage);
            }

            return MessageBox.Show(messageBoxText, caption, button, messageBoxImage);
        }

        private static void Save(ImmutableList<Bookmark> simulation, string? fileName = null)
        {
            if (!Directory.Exists(BookmarksDirectory))
            {
                Directory.CreateDirectory(BookmarksDirectory);
            }

            if (File() is { } file)
            {
                System.IO.File.WriteAllText(file.FullName, JsonSerializer.Serialize(simulation));
            }

            FileInfo? File()
            {
                if (fileName is { })
                {
                    return new(fileName);
                }

                var dialog = new SaveFileDialog
                {
                    InitialDirectory = BookmarksDirectory,
                    FileName = $"Bookmarks {DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
                    DefaultExt = ".bookmarks",
                    Filter = "Bookmark files|*.bookmarks",
                };

                if (dialog.ShowDialog() is true)
                {
                    return new(dialog.FileName);
                }

                return null;
            }
        }

        private void AskAndSave()
        {
            if (this.DataContext is MainViewModel { Bookmarks: { } bookmarks } &&
                ShowMessageBox("Do you want to save current simulation first?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Save(bookmarks);
            }
        }

        private void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel mainViewModel)
            {
                this.AskAndSave();
                mainViewModel.Bookmarks = ImmutableList<Bookmark>.Empty;
                e.Handled = true;
            }
        }

        private void OnOpen(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel mainViewModel)
            {
                this.AskAndSave();
                if (!Directory.Exists(BookmarksDirectory))
                {
                    Directory.CreateDirectory(BookmarksDirectory);
                }

                var dialog = new OpenFileDialog
                {
                    InitialDirectory = BookmarksDirectory,
                    Filter = "Bookmark files|*.bookmarks",
                };

                if (dialog.ShowDialog() is true)
                {
                    try
                    {
                        mainViewModel.Bookmarks = JsonSerializer.Deserialize<ImmutableList<Bookmark>>(File.ReadAllText(dialog.FileName));
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        _ = ShowMessageBox(exception.Message, "Invalid bookmark file.", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    e.Handled = true;
                }
            }
        }

        private void OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel viewModel)
            {
                this.AskAndSave();
                viewModel.Bookmarks = null;
                e.Handled = true;
            }
        }

        private void OnCanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel { Bookmarks: { } })
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel { Bookmarks: { } bookmarks })
            {
                Save(bookmarks);
                e.Handled = true;
            }
        }
    }
}
