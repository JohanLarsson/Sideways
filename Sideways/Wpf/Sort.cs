namespace Sideways
{
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;

    public static class Sort
    {
        public static readonly DependencyProperty DirectionProperty = DependencyProperty.RegisterAttached(
            "Direction",
            typeof(ListSortDirection?),
            typeof(Sort),
            new PropertyMetadata(
                default(ListSortDirection?),
                OnDirectionChanged));

        [AttachedPropertyBrowsableForType(typeof(ItemsControl))]
        public static ListSortDirection? GetDirection(ItemsControl element)
        {
            return (ListSortDirection)element.GetValue(DirectionProperty);
        }

        public static void SetDirection(ItemsControl element, ListSortDirection? value)
        {
            element.SetValue(DirectionProperty, value);
        }

        private static void OnDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ItemsControl { Items: { } items })
            {
                if (e.NewValue is ListSortDirection direction)
                {
                    items.SortDescriptions.Add(new SortDescription(string.Empty, direction));
                }
                else if (e.OldValue is ListSortDirection old &&
                         items.SortDescriptions.FirstOrDefault(x => x.Direction == old) is { } oldDescription)
                {
                    items.SortDescriptions.Remove(oldDescription);
                }
            }
        }
    }
}
