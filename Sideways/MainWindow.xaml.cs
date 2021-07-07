namespace Sideways
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
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
                case Key.Left
                    when this.DataContext is MainViewModel vm &&
                         ShouldUpdateChart():
                    vm.SkipLeftCommand.Execute(Interval(Keyboard.Modifiers));
                    e.Handled = true;
                    break;
                case Key.Right
                    when this.DataContext is MainViewModel vm &&
                         ShouldUpdateChart():
                    vm.SkipRightCommand.Execute(Interval(Keyboard.Modifiers));
                    e.Handled = true;
                    break;
            }

            if (e.SystemKey == Key.S &&
                Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                var window = new Window
                {
                    Content = new SyncView(),
                    Owner = this,
                };

                window.Show();
                e.Handled = true;
            }

            base.OnPreviewKeyDown(e);

            bool ShouldUpdateChart()
            {
                return FocusManager.GetFocusedElement(this) switch
                {
                    TextBox => this.SymbolBox.IsKeyboardFocusWithin,
                    _ => true,
                };
            }

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
            Keyboard.Focus(this.SymbolBox);
        }

        private void OnCopy(object sender, RoutedEventArgs e)
        {
            Clipboard.SetImage(Capture(this.Root, Rect()));
            e.Handled = true;

            Rect Rect()
            {
                return new(
                    default(Point) + VisualTreeHelper.GetOffset(this.Root),
                    new Size(
                        this.Root.ActualWidth - this.ContextPane.ActualWidth,
                        this.Root.ActualHeight));
            }

            // Workaround for WPF weirdness https://docs.microsoft.com/en-us/archive/blogs/jaimer/rendertargetbitmap-tips
            static BitmapSource Capture(Visual target, Rect bounds)
            {
                var rtb = new RenderTargetBitmap(
                    (int)bounds.Width,
                    (int)bounds.Height,
                    96.0,
                    96.0,
                    PixelFormats.Pbgra32);

                var dv = new DrawingVisual();
                using (var ctx = dv.RenderOpen())
                {
                    var vb = new VisualBrush(target)
                    {
                        ViewboxUnits = BrushMappingMode.Absolute,
                        Viewbox = bounds,
                    };

                    ctx.DrawRectangle(vb, null, new Rect(bounds.Size));
                }

                rtb.Render(dv);
                return rtb;
            }
        }

        private void OnClickMinimize(object sender, RoutedEventArgs e) => this.SetCurrentValue(WindowStateProperty, WindowState.Minimized);

        private void OnClickMaximizeRestore(object sender, RoutedEventArgs e) => this.SetCurrentValue(WindowStateProperty, this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized);

        private void OnClickClose(object sender, RoutedEventArgs e) => this.Close();

        private void CanAddBookMark(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.DataContext is MainViewModel { Bookmarks: { SelectedBookmarkFile: not null } };
            e.Handled = true;
        }

        private void AddBookMark(object sender, ExecutedRoutedEventArgs e)
        {
            if (sender is MeasureDecorator decorator &&
                this.DataContext is MainViewModel { } vm &&
                decorator.TimeAndPrice(Mouse.GetPosition(decorator)) is { } timeAndPrice)
            {
                vm.AddBookmark(timeAndPrice.Time);
            }
        }
    }
}
