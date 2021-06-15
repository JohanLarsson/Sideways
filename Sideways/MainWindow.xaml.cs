namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Threading;

    public partial class MainWindow : Window
    {
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
                case Key.B
                    when Keyboard.Modifiers == ModifierKeys.Control &&
                         this.DataContext is MainViewModel { CurrentSymbol: { Symbol: { } symbol }, Time: { } time, Bookmarks: { SelectedBookmarkFile: { } bookmarkFile } }:
                    bookmarkFile.Add(new Bookmark(symbol, time, ImmutableSortedSet<string>.Empty, null));
                    e.Handled = true;
                    break;
                case Key.B
                    when Keyboard.Modifiers == ModifierKeys.Control:
                    MessageBox.Show(this, "No bookmark added, select a symbol and a bookmarks file.", "Bookmark", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case Key.W
                    when Keyboard.Modifiers == ModifierKeys.Control &&
                         this.DataContext is MainViewModel { CurrentSymbol: { Symbol: { } symbol } } vm:
                    vm.WatchList.Add(symbol);
                    e.Handled = true;
                    break;
                case Key.W
                    when Keyboard.Modifiers == ModifierKeys.Control:
                    MessageBox.Show(this, "No watchlist added.", "Watchlist", MessageBoxButton.OK, MessageBoxImage.Information);
                    break;
                case Key.Left
                    when Keyboard.Modifiers == ModifierKeys.Shift:
                    Skip(CandleInterval.Hour, this.timer is null ? -1 : -2);
                    e.Handled = true;
                    break;
                case Key.Left:
                    Skip(CandleInterval.Day, this.timer is null ? -1 : -2);
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

        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.Settings.Save();
                foreach (var bookmarkFile in mainViewModel.Bookmarks.BookmarkFiles)
                {
                    bookmarkFile.AskSave();
                }
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
