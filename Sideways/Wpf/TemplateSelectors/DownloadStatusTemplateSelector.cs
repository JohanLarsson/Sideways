namespace Sideways
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;

    using Sideways.AlphaVantage;

    public class DownloadStatusTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? Unknown { get; set; }

        public DataTemplate? Waiting { get; set; }

        public DataTemplate? Running { get; set; }

        public DataTemplate? Completed { get; set; }

        public DataTemplate? Error { get; set; }

        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            return item switch
            {
                DownloadStatus.Unknown => this.Unknown,
                DownloadStatus.Waiting => this.Waiting,
                DownloadStatus.Running => this.Running,
                DownloadStatus.Completed => this.Completed,
                DownloadStatus.Error => this.Error,
                DownloadStatus => throw new InvalidEnumArgumentException(nameof(item), (int)item, typeof(DownloadStatus)),
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
