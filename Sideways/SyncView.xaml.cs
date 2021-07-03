namespace Sideways
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Microsoft.Win32;

    public partial class SyncView : UserControl
    {
        public SyncView()
        {
            this.InitializeComponent();
        }

        private void OnOpen(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog(Window.GetWindow(this)) is true)
            {
                ((SyncViewModel)this.DataContext).FileName = dialog.FileName;
            }
        }
    }
}
