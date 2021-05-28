namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.IO;
    using System.Text.Json;
    using System.Windows;
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

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.C
                    when Keyboard.Modifiers == ModifierKeys.Control:
                    ApplicationCommands.Copy.Execute(null, this);
                    e.Handled = true;
                    break;
                case Key.Left
                    when Keyboard.Modifiers == ModifierKeys.Shift:
                    Skip(CandleInterval.Hour, -1);
                    e.Handled = true;
                    break;
                case Key.Left:
                    Skip(CandleInterval.Day, -1);
                    e.Handled = true;
                    break;
                case Key.Right
                    when Keyboard.Modifiers == ModifierKeys.Shift:
                    Skip(CandleInterval.Hour, 1);
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
                        Interval = TimeSpan.FromMilliseconds(50),
                    };
                    this.timer.Tick += (_, _) => Skip(CandleInterval.Minute, 1);
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
                    DefaultExt = ".sim",
                    Filter = "Log files|*.sim",
                };

                if (dialog.ShowDialog() is true)
                {
                    return new(dialog.FileName);
                }

                return null;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this.SymbolComboBox);
        }

        private void OnCopy(object sender, RoutedEventArgs e)
        {
            var bmp = new RenderTargetBitmap((int)this.Charts.ActualWidth, (int)this.Charts.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(this);
            Clipboard.SetImage(bmp);
            e.Handled = true;
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
                if (MessageBox.Show(this, "Do you want to save current simulation first?", "Simulation", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    Save(simulation, DateTimeOffsetExtensions.Max(time, simulation.Time));
                }

                viewModel.UpdateSimulation(null);
                e.Handled = true;
            }
        }

        private void OnOpenSimulation(object sender, ExecutedRoutedEventArgs e)
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

        private void OnOpenDownloader(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new Window
            {
                Title = "Downloader",
                SizeToContent = SizeToContent.Width,
                Height = 500,
                Content = new DownloadView
                {
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                    DataContext = ((MainViewModel)this.DataContext).Downloader,
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
                },
                Owner = this,
            };
            window.Show();
        }

        private void OnOpenSettings(object sender, ExecutedRoutedEventArgs e)
        {
            var settings = ((MainViewModel)this.DataContext).Settings;
            var window = new Window
            {
                Title = "Settings",
                SizeToContent = SizeToContent.Width,
                Height = 500,
                Content = new SettingsView
                {
#pragma warning disable IDISP003 // Dispose previous before re-assigning.
                    DataContext = settings,
#pragma warning restore IDISP003 // Dispose previous before re-assigning.
                },
                Owner = this,
            };
            window.Show();
            settings.Save();
        }
    }
}
