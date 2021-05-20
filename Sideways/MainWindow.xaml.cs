namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Text.Json;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    using Microsoft.Win32;

    public partial class MainWindow : Window
    {
        private static readonly string SimulationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Sideways", "Simulations");

        private DispatcherTimer? timer;

        public MainWindow()
        {
            this.InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.DataContext = new MainViewModel();
            }
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (Keyboard.FocusedElement is TextBox focusedElement &&
                this.SymbolComboBox.IsAncestorOf(focusedElement))
            {
                Keyboard.ClearFocus();
                Keyboard.Focus(this.SymbolComboBox);
                e.Handled = true;
            }

            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.C
                    when Keyboard.Modifiers == ModifierKeys.Control:
                    this.OnCopy(this, new RoutedEventArgs(CommandManager.ExecutedEvent));
                    e.Handled = true;
                    break;
                case Key.Left:
                    Skip(CandleInterval.Day, -1);
                    e.Handled = true;
                    break;
                case Key.Right:
                    Skip(CandleInterval.Day, 1);
                    e.Handled = true;
                    break;
                case Key.Space
                    when this.timer is null:
                    this.timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(20),
                    };
                    this.timer.Tick += (_, _) => Skip(CandleInterval.Hour, 1);
                    this.timer.Start();
                    e.Handled = true;
                    break;
                case Key.Space:
                    this.timer.Stop();
                    this.timer = null;
                    e.Handled = true;
                    break;
            }

            base.OnPreviewKeyDown(e);

            void Skip(CandleInterval interval, int count)
            {
                if (this.DataContext is MainViewModel { CurrentSymbol: { Candles: { } candles } } vm)
                {
                    vm.Time = candles.Skip(vm.Time, interval, count);
                }
            }
        }

        private static void Save(Simulation simulation, DateTimeOffset time)
        {
            if (!Directory.Exists(SimulationDirectory))
            {
                Directory.CreateDirectory(SimulationDirectory);
            }

            var dialog = new SaveFileDialog
            {
                InitialDirectory = SimulationDirectory,
                FileName = simulation.Name,
                DefaultExt = ".sim",
                Filter = "Log files|*.sim",
            };

            if (dialog.ShowDialog() is true)
            {
                simulation.Time = time;
                File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(simulation));
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this.SymbolComboBox);
        }

        private void OnCopy(object sender, RoutedEventArgs e)
        {
            var bmp = new RenderTargetBitmap((int)this.ChartArea.ActualWidth, (int)this.ChartArea.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(this);
            Clipboard.SetImage(bmp);
            e.Handled = true;
        }

        private void OnCanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel { Simulation: { Name: { } } })
            {
                e.CanExecute = true;
                e.Handled = true;
            }
        }

        private void OnSave(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel { Time: var time, Simulation: { } simulation })
            {
                Save(simulation, time);
                e.Handled = true;
            }
        }

        private void OnOpen(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is MainViewModel { } mainViewModel)
            {
                if (mainViewModel is { Time: var time, Simulation: { } simulation } &&
                    MessageBox.Show(this, "Do you want to save current simulation first?", "Simulation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
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
                    Filter = "Log files|*.sim",
                };

                if (dialog.ShowDialog() is true)
                {
                    var sim = JsonSerializer.Deserialize<Simulation>(File.ReadAllText(dialog.FileName));
                    mainViewModel.UpdateSimulation(sim);
                    e.Handled = true;
                }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key is Key.Enter or Key.Return or Key.Tab &&
                ReferenceEquals(sender, this.SymbolComboBox))
            {
                BindingOperations.GetBindingExpression(this.SymbolComboBox, ComboBox.TextProperty)!.UpdateSource();
                Keyboard.ClearFocus();
                Keyboard.Focus(this.SymbolComboBox);
                e.Handled = true;
            }
        }
    }
}
