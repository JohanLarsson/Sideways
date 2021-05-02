namespace Sideways
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using System.Windows;

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            if (DesignerProperties.GetIsInDesignMode(this))
            {
#pragma warning disable VSTHRD110 // Observe result of async calls
                _ = Load("TSLA");
#pragma warning restore VSTHRD110 // Observe result of async calls
            }
            else
            {
#pragma warning disable VSTHRD110 // Observe result of async calls
                _ = Load("TSLA");
#pragma warning restore VSTHRD110 // Observe result of async calls
            }

            async Task Load(string symbol)
            {
                var vm = (MainViewModel)this.DataContext;
                await vm.LoadAsync(symbol).ConfigureAwait(false);
            }
        }
    }
}
