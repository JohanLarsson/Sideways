namespace Sideways
{
    using System.Windows;
    using System.Windows.Controls;
    using Sideways.AlphaVantage;

    public class SymbolDownloadTemplateSelector : DataTemplateSelector
    {
        public HierarchicalDataTemplate? Symbol { get; set; }

        public DataTemplate? Days { get; set; }

        public DataTemplate? Minutes { get; set; }

        public DataTemplate? Earnings { get; set; }

        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            return item switch
            {
                SymbolDownloads => this.Symbol,
                DaysDownload => this.Days,
                MinutesDownload => this.Minutes,
                EarningsDownload => this.Earnings,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
