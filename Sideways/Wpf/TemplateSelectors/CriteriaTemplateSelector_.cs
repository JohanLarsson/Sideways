namespace Sideways
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Markup;

    [ContentProperty(nameof(Templates))]
    public class CriteriaTemplateSelector_ : DataTemplateSelector
    {
#pragma warning disable CA1002 // Do not expose generic lists
        public List<DataTemplate> Templates { get; } = new();
#pragma warning restore CA1002 // Do not expose generic lists

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            return this.Templates.SingleOrDefault(x => Equals(x.DataType, item?.GetType())) ??
                   base.SelectTemplate(item, container);
        }
    }
}
