namespace Sideways
{
    using System;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    using Microsoft.Win32;

    public partial class SimulationView : UserControl
    {
        public SimulationView()
        {
            this.InitializeComponent();
        }

        private void OnNew(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is SimulationViewModel viewModel)
            {
                viewModel.StartNew();
                e.Handled = true;
            }
        }

        private void OnOpen(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is SimulationViewModel viewModel)
            {
                viewModel.AskSave();

                if (!Directory.Exists(SimulationViewModel.Directory))
                {
                    Directory.CreateDirectory(SimulationViewModel.Directory);
                }

                var dialog = new OpenFileDialog
                {
                    InitialDirectory = SimulationViewModel.Directory,
                    Filter = "Simulation files|*.simulation;*.sim",
                };

                if (dialog.ShowDialog() is true)
                {
                    try
                    {
                        viewModel.Load(new FileInfo(dialog.FileName));
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        _ = MessageBox.Show(exception.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    e.Handled = true;
                }
            }
        }

        private void OnClose(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is SimulationViewModel viewModel)
            {
                viewModel.Close();
                e.Handled = true;
            }
        }

        private void OnCanSaveOrClose(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.DataContext is SimulationViewModel { Current: { } })
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is SimulationViewModel { Current: { } } viewModel)
            {
                viewModel.Save();
                e.Handled = true;
            }
        }
    }
}
