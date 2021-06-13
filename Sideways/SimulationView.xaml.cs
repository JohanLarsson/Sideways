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
        private static readonly string SimulationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Simulations");

        public SimulationView()
        {
            this.InitializeComponent();
        }

        private static void Save(Simulation simulation, DateTimeOffset time, string? fileName = null)
        {
            if (!Directory.Exists(SimulationDirectory))
            {
                Directory.CreateDirectory(SimulationDirectory);
            }

            if (File() is { } file)
            {
                simulation.Time = time;
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
                    InitialDirectory = SimulationDirectory,
                    FileName = $"Simulation {DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)}",
                    DefaultExt = ".simulation",
                    Filter = "Simulation files|*.simulation;*.sim",
                };

                if (dialog.ShowDialog() is true)
                {
                    return new(dialog.FileName);
                }

                return null;
            }
        }

        private static MessageBoxResult ShowMessageBox(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage messageBoxImage = MessageBoxImage.None)
        {
            if (Application.Current.MainWindow is { } window)
            {
                return MessageBox.Show(window, messageBoxText, caption, button, messageBoxImage);
            }

            return MessageBox.Show(messageBoxText, caption, button, messageBoxImage);
        }

        private void OnCanSaveSimulation(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel { Simulation: { } })
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private void OnSaveSimulation(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel { Time: var time, Simulation: { } simulation })
            {
                Save(simulation, DateTimeOffsetExtensions.Max(time, simulation.Time));
                e.Handled = true;
            }
        }

        private void OnCloseSimulation(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel { Time: var time, Simulation: { } simulation } viewModel)
            {
                if (ShowMessageBox("Do you want to save current simulation first?", "Simulation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Save(simulation, DateTimeOffsetExtensions.Max(time, simulation.Time));
                }

                viewModel.UpdateSimulation(null);
                e.Handled = true;
            }
        }

        private void OnOpenSimulation(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel mainViewModel)
            {
                if (mainViewModel is { Time: var time, Simulation: { } simulation } &&
                    ShowMessageBox("Do you want to save current simulation first?", "Simulation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Save(simulation, time);
                }

                if (!Directory.Exists(SimulationDirectory))
                {
                    Directory.CreateDirectory(SimulationDirectory);
                }

                var dialog = new OpenFileDialog
                {
                    InitialDirectory = SimulationDirectory,
                    Filter = "Simulation files|*.simulation;*.sim",
                };

                if (dialog.ShowDialog() is true)
                {
                    try
                    {
                        mainViewModel.UpdateSimulation(JsonSerializer.Deserialize<Simulation>(File.ReadAllText(dialog.FileName)));
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
    }
}
