﻿namespace Sideways
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
        private static readonly string Directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Bookmarks");

        private FileInfo? file;

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

        private void Save(ImmutableList<Bookmark> simulation)
        {
            if (!System.IO.Directory.Exists(Directory))
            {
                System.IO.Directory.CreateDirectory(Directory);
            }

            if (File() is { FullName: { } fileName })
            {
                System.IO.File.WriteAllText(fileName, JsonSerializer.Serialize(simulation));
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
                    FileName = $"Bookmarks {DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
                    DefaultExt = ".bookmarks",
                    Filter = "Bookmark files|*.bookmarks",
                };

                if (dialog.ShowDialog() is true)
                {
                    this.file = new FileInfo(dialog.FileName);
                    return new(dialog.FileName);
                }

                return null;
            }
        }

        private void AskSave()
        {
            if (this.DataContext is MainViewModel { Bookmarks: { } bookmarks } &&
                IsDirty() &&
                ShowMessageBox("Do you want to save current bookmarks first?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.Save(bookmarks);
            }

            bool IsDirty()
            {
                return this.file is { FullName: { } fileName } &&
                       JsonSerializer.Serialize(bookmarks, new JsonSerializerOptions { WriteIndented = true }) != File.ReadAllText(fileName);
            }
        }

        private void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel mainViewModel)
            {
                this.AskSave();
                mainViewModel.Bookmarks = ImmutableList<Bookmark>.Empty;
                e.Handled = true;
            }
        }

        private void OnOpen(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel mainViewModel)
            {
                this.AskSave();
                if (!System.IO.Directory.Exists(Directory))
                {
                    System.IO.Directory.CreateDirectory(Directory);
                }

                var dialog = new OpenFileDialog
                {
                    InitialDirectory = Directory,
                    Filter = "Bookmark files|*.bookmarks",
                };

                if (dialog.ShowDialog() is true)
                {
                    try
                    {
                        mainViewModel.Bookmarks = JsonSerializer.Deserialize<ImmutableList<Bookmark>>(File.ReadAllText(dialog.FileName));
                        this.file = new FileInfo(dialog.FileName);
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
                this.AskSave();
                viewModel.Bookmarks = null;
                this.file = null;
                e.Handled = true;
            }
        }

        private void OnCanSaveOrClose(object sender, CanExecuteRoutedEventArgs e)
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
                this.Save(bookmarks);
                e.Handled = true;
            }
        }
    }
}