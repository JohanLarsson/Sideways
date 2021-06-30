namespace Sideways
{
    using System.Windows;
    using System.Windows.Controls;

    public sealed class CriteriaTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? AdrCriteria { get; set; }

        public DataTemplate? AtrCriteria { get; set; }

        public DataTemplate? AverageDollarVolumeCriteria { get; set; }

        public DataTemplate? AverageVolumeCriteria { get; set; }

        public DataTemplate? HasMinutesCriteria { get; set; }

        public DataTemplate? PriceCriteria { get; set; }

        public DataTemplate? TimeCriteria { get; set; }

        public DataTemplate? YieldCriteria { get; set; }

        public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
        {
            return item switch
            {
                Scan.AdrCriteria => this.AdrCriteria,
                Scan.AtrCriteria => this.AtrCriteria,
                Scan.AverageDollarVolumeCriteria => this.AverageDollarVolumeCriteria,
                Scan.AverageVolumeCriteria => this.AverageVolumeCriteria,
                Scan.HasMinutesCriteria => this.HasMinutesCriteria,
                Scan.PriceCriteria => this.PriceCriteria,
                Scan.TimeCriteria => this.TimeCriteria,
                Scan.YieldCriteria => this.YieldCriteria,
                _ => base.SelectTemplate(item, container),
            };
        }
    }
}
