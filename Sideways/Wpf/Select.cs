namespace Sideways
{
    using System.Windows;
    using System.Windows.Controls;

    public static class Select
    {
        public static readonly DependencyProperty ToggleProperty = DependencyProperty.RegisterAttached(
            "Toggle",
            typeof(bool),
            typeof(Select),
            new PropertyMetadata(default(bool)));

        static Select()
        {
            EventManager.RegisterClassHandler(typeof(DataGridCell), UIElement.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(OnPreviewMouseLeftButtonDown));

            static void OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
            {
                switch (sender)
                {
                    case DataGridCell { IsSelected: true, IsEditing: false, Column: { } column } cell
                        when GetToggle(column) &&
                             cell.FirstAncestor<DataGridRow>() is { } row &&
                             row.FirstAncestor<DataGrid>() is { } grid:
                        switch (grid.SelectionUnit)
                        {
                            case DataGridSelectionUnit.FullRow:
                                row.IsSelected = false;
                                break;
                            case DataGridSelectionUnit.Cell:
                                cell.IsSelected = false;
                                break;
                        }

                        e.Handled = true;
                        break;
                }
            }
        }

        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetToggle(DependencyObject element) => (bool)element.GetValue(ToggleProperty);

        public static void SetToggle(DependencyObject element, bool value) => element.SetValue(ToggleProperty, value);
    }
}
