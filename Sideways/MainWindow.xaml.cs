namespace Sideways
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                ((MainViewModel)this.DataContext).Load("TSLA");
            }
            else
            {
                ((MainViewModel)this.DataContext).Load("TSLA");
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    Skip(-1);
                    e.Handled = true;
                    break;
                case Key.Right:
                    Skip(1);
                    e.Handled = true;
                    break;
            }

            base.OnPreviewKeyDown(e);

            void Skip(int count)
            {
                if (this.DataContext is MainViewModel { CurrentSymbol: { Candles: { } candles } } vm)
                {
                    vm.EndTime = candles.Skip(vm.EndTime, CandleInterval.Day, count);
                }
            }
        }
    }
}
