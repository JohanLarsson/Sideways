namespace Sideways
{
    using System.ComponentModel;
    using System.Windows;

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
    }
}
