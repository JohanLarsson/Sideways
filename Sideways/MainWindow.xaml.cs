namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Windows;
    using System.Windows.Controls;
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
                var mainViewModel = new MainViewModel();
                mainViewModel.CurrentSymbol = mainViewModel.Symbols.FirstOrDefault();
                this.DataContext = mainViewModel;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
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
                    break;
                case Key.Space:
                    this.timer.Stop();
                    this.timer = null;
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

        private void OnCopy(object sender, ExecutedRoutedEventArgs e)
        {
            var bmp = new RenderTargetBitmap((int)this.ChartArea.ActualWidth, (int)this.ChartArea.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(this);
            Clipboard.SetImage(bmp);
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
                    mainViewModel.Simulation = sim;
                    mainViewModel.Time = sim.Time ?? throw new InvalidOperationException("Missing time in simulation.");
                    e.Handled = true;
                }
            }
        }

        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                _ = this.Dispatcher.BeginInvoke(new Action(() => textBox.SelectAll()));
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox &&
                e.Key == Key.Enter)
            {
                textBox.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
        }
    }
}
