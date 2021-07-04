namespace Sideways
{
    using System.Windows;
    using System.Windows.Controls;

    public static class Toggle
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.RegisterAttached(
            "IsSelected",
            typeof(bool),
            typeof(Toggle),
            new PropertyMetadata(false));

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.RegisterAttached(
            "IsChecked",
            typeof(bool),
            typeof(Toggle),
            new PropertyMetadata(false));

        static Toggle()
        {
            EventManager.RegisterClassHandler(typeof(DataGridCell), UIElement.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(OnPreviewMouseLeftButtonDown));
            EventManager.RegisterClassHandler(typeof(RadioButton), UIElement.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(OnPreviewMouseLeftButtonDown));

            static void OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
            {
                switch (sender)
                {
                    case DataGridCell { IsSelected: true, IsEnabled: true, IsEditing: false, Column: { } column } cell
                        when GetIsSelected(column) &&
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
                    case RadioButton { IsChecked: true, IsEnabled: true } radioButton:
                        radioButton.IsChecked = false;
                        e.Handled = true;
                        break;
                }
            }
        }

        [AttachedPropertyBrowsableForType(typeof(DataGridCell))]
        public static bool GetIsSelected(DependencyObject element) => (bool)element.GetValue(IsSelectedProperty);

        public static void SetIsSelected(DependencyObject element, bool value) => element.SetValue(IsSelectedProperty, value);

        [AttachedPropertyBrowsableForType(typeof(RadioButton))]
        public static bool GetIsChecked(DependencyObject element) => (bool)element.GetValue(IsCheckedProperty);

        public static void SetIsChecked(DependencyObject element, bool value) => element.SetValue(IsCheckedProperty, value);
    }
}
