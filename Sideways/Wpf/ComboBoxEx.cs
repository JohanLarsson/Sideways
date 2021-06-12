namespace Sideways
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    public static class ComboBoxEx
    {
        public static readonly DependencyProperty IsSymbolSearchProperty = DependencyProperty.RegisterAttached(
            "IsSymbolSearch",
            typeof(bool),
            typeof(ComboBoxEx),
            new PropertyMetadata(default(bool)));

        static ComboBoxEx()
        {
            EventManager.RegisterClassHandler(typeof(ComboBox), UIElement.KeyDownEvent, new KeyEventHandler((_, e) => OnKeyDown(e)));
            EventManager.RegisterClassHandler(typeof(ComboBox), UIElement.KeyUpEvent, new KeyEventHandler((_, e) => OnKeyUp(e)));
            EventManager.RegisterClassHandler(typeof(ComboBox), UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler((_, e) => OnPreviewMouseLeftButtonDown(e)));

            static void OnKeyDown(KeyEventArgs e)
            {
                if (e.Source is ComboBox comboBox &&
                    GetIsSymbolSearch(comboBox))
                {
                    switch (e.Key)
                    {
                        case Key.Enter or Key.Return or Key.Tab:
                            BindingOperations.GetBindingExpression(comboBox, ComboBox.TextProperty)!.UpdateSource();
                            Keyboard.ClearFocus();
                            Keyboard.Focus(comboBox);
                            e.Handled = true;
                            break;
                    }
                }
            }

            static void OnKeyUp(KeyEventArgs e)
            {
                if (e.Source is ComboBox comboBox &&
                    GetIsSymbolSearch(comboBox))
                {
                    switch (e)
                    {
                        case { Key: Key.Up or Key.Down }:
                        case { SystemKey: Key.Left or Key.Right or Key.R }:
                            // We toggle focus so following keyboard input overwrites current text.
                            Keyboard.ClearFocus();
                            Keyboard.Focus(comboBox);
                            e.Handled = true;
                            break;
                    }
                }
            }

            static void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
            {
                if (e.Source is ComboBox comboBox &&
                    e.OriginalSource.GetType().Name == "TextBoxView" &&
                    GetIsSymbolSearch(comboBox))
                {
                    // We toggle focus so that current text is selected so that following keyboard input is not where clicked but starts fresh.
                    Keyboard.ClearFocus();
                    Keyboard.Focus(comboBox);
                    e.Handled = true;
                }
            }
        }

        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetIsSymbolSearch(DependencyObject element)
        {
            return (bool)element.GetValue(IsSymbolSearchProperty);
        }

        public static void SetIsSymbolSearch(DependencyObject element, bool value)
        {
            element.SetValue(IsSymbolSearchProperty, value);
        }
    }
}
