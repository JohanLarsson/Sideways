namespace Sideways
{
    using System.Windows;
    using System.Windows.Controls;

    public static class SingleClick
    {
        public static readonly DependencyProperty ToggleProperty = DependencyProperty.RegisterAttached(
            "Toggle",
            typeof(bool),
            typeof(SingleClick),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.Inherits));

        static SingleClick()
        {
            EventManager.RegisterClassHandler(typeof(DataGridCell), UIElement.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(OnPreviewMouseLeftButtonDown));

            static void OnPreviewMouseLeftButtonDown(object sender, RoutedEventArgs e)
            {
                if (sender is DataGridCell { IsReadOnly: false, IsEditing: false, Content: CheckBox { IsEnabled: true } checkBox } cell &&
                    GetToggle(cell))
                {
                    checkBox.IsChecked = !checkBox.IsChecked;
                    e.Handled = true;
                }
            }
        }

        public static void SetToggle(DependencyObject element, bool value) => element.SetValue(ToggleProperty, value);

        public static bool GetToggle(DependencyObject element) => (bool)element.GetValue(ToggleProperty);
    }
}
