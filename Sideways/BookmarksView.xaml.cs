namespace Sideways
{
    using System;
    using System.Collections.Immutable;
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

        private void OnOpen(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel mainViewModel)
            {
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
    }
}
