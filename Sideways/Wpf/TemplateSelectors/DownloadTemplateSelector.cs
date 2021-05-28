namespace Sideways
{
    using System.Windows;
    using System.Windows.Controls;
    using Sideways.AlphaVantage;

    public class DownloadTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? Days { get; set; }

        public DataTemplate? Minutes { get; set; }

        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            return item switch
            {
                DaysDownload => this.Days,
                MinutesDownload => this.Minutes,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
