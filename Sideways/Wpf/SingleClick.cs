namespace Sideways
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Media;

    public static class SingleClick
    {
        public static readonly DependencyProperty ToggleProperty = DependencyProperty.RegisterAttached(
            "Toggle",
            typeof(bool),
            typeof(SingleClick),
            new PropertyMetadata(false));

        static SingleClick()
        {
            EventManager.RegisterClassHandler(typeof(DataGridCell), UIElement.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(OnPreviewMouseLeftButtonDown));

            static void OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
            {
                switch (sender)
                {
                    case DataGridCell { IsReadOnly: false, IsEditing: false, Content: CheckBox { IsEnabled: true } checkBox, Column: DataGridCheckBoxColumn column } cell
                        when GetToggle(column):
                        checkBox.IsChecked = !checkBox.IsChecked;
                        BindingOperations.GetBindingExpression(checkBox, ToggleButton.IsCheckedProperty)?.UpdateSource();
                        e.Handled = true;
                        break;
                    case DataGridCell { IsSelected: true, IsEditing: false, Column: { } column } cell
                        when GetToggle(column) &&
                             FirstAncestor<DataGridRow>(cell) is { } row &&
                             FirstAncestor<DataGrid>(row) is { } grid:
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

            static T? FirstAncestor<T>(Visual child)
                where T : Visual
            {
                var parent = VisualTreeHelper.GetParent(child);
                while (parent is { })
                {
                    if (parent is T match)
                    {
                        return match;
                    }

                    parent = VisualTreeHelper.GetParent(parent);
                }

                return null;
            }
        }

        public static void SetToggle(DataGridColumn element, bool value) => element.SetValue(ToggleProperty, value);

        public static bool GetToggle(DataGridColumn element) => (bool)element.GetValue(ToggleProperty);
    }
}
