namespace Sideways
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;

    public partial class MainWindow : Window
    {
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
                    when this.DataContext is MainViewModel vm:
                    vm.SkipLeftCommand.Execute(Interval(Keyboard.Modifiers));
                    e.Handled = true;
                    break;
                case Key.Right
                    when this.DataContext is MainViewModel vm:
                    vm.SkipRightCommand.Execute(Interval(Keyboard.Modifiers));
                    e.Handled = true;
                    break;
                case Key.Space
                    when this.DataContext is MainViewModel vm:
                    vm.Animation.ToggleCommand.Execute(null);
                    e.Handled = true;
                    break;
            }

            base.OnPreviewKeyDown(e);

            static CandleInterval Interval(ModifierKeys modifier)
            {
                return modifier switch
                {
                    ModifierKeys.Shift => CandleInterval.Hour,
                    ModifierKeys.Control => CandleInterval.FifteenMinutes,
                    ModifierKeys.Control | ModifierKeys.Shift => CandleInterval.Minute,
                    _ => CandleInterval.Day,
                };
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.Settings.Save();
                foreach (var bookmarkFile in mainViewModel.Bookmarks.BookmarkFiles)
                {
                    bookmarkFile.AskSave();
                }

                mainViewModel.Simulation.AskSave();
            }

            base.OnClosing(e);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Keyboard.Focus(this.SymbolComboBox);
        }

        private void OnCopy(object sender, RoutedEventArgs e)
        {
            var bmp = new RenderTargetBitmap((int)this.Charts.ActualWidth, (int)this.Charts.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(this.Charts);
            Clipboard.SetImage(bmp);
            e.Handled = true;
        }

        private void OnClickMinimize(object sender, RoutedEventArgs e) => this.SetCurrentValue(WindowStateProperty, WindowState.Minimized);

        private void OnClickMaximizeRestore(object sender, RoutedEventArgs e) => this.SetCurrentValue(WindowStateProperty, this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);

        private void OnClickClose(object sender, RoutedEventArgs e) => this.Close();
    }
}
