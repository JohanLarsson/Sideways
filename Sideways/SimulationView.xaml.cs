namespace Sideways
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Text.Json;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Microsoft.Win32;

    public partial class SimulationView : UserControl
    {
        private static readonly string Directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Simulations");

        private FileInfo? file;

        public SimulationView()
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

        private void Save(Simulation simulation, DateTimeOffset time)
        {
            if (!System.IO.Directory.Exists(Directory))
            {
                System.IO.Directory.CreateDirectory(Directory);
            }

            if (File() is { FullName: { } fullName })
            {
                simulation.Time = time;
                System.IO.File.WriteAllText(fullName, JsonSerializer.Serialize(simulation));
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
                    FileName = $"Simulation {DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
                    DefaultExt = ".simulation",
                    Filter = "Simulation files|*.simulation;*.sim",
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
            if (this.DataContext is MainViewModel { Time: var time, Simulation: { } simulation } &&
                IsDirty() &&
                ShowMessageBox("Do you want to save current simulation first?", "Save", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.Save(simulation, time);
            }

            bool IsDirty()
            {
                return this.file is { FullName: { } fileName } &&
                       JsonSerializer.Serialize(simulation, new JsonSerializerOptions { WriteIndented = true }) != File.ReadAllText(fileName);
            }
        }

        private void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel mainViewModel)
            {
                this.AskSave();
                mainViewModel.UpdateSimulation(Simulation.Create(mainViewModel.Time));
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
                    Filter = "Simulation files|*.simulation;*.sim",
                };

                if (dialog.ShowDialog() is true)
                {
                    try
                    {
                        mainViewModel.UpdateSimulation(JsonSerializer.Deserialize<Simulation>(File.ReadAllText(dialog.FileName)));
                        this.file = new FileInfo(dialog.FileName);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        _ = ShowMessageBox(exception.Message, "Invalid simulation file.", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    e.Handled = true;
                }
            }
        }

        private void OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel { Simulation: { } } viewModel)
            {
                this.AskSave();
                viewModel.UpdateSimulation(null);
                this.file = null;
                e.Handled = true;
            }
        }

        private void OnCanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel { Simulation: { } })
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel { Time: var time, Simulation: { } simulation })
            {
                this.Save(simulation, DateTimeOffsetExtensions.Max(time, simulation.Time));
                e.Handled = true;
            }
        }
    }
}
