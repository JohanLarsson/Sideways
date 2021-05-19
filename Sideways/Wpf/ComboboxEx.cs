namespace Sideways
{
    using System.Windows;
    using System.Windows.Controls;

    public static class ComboboxEx
    {
        public static readonly DependencyProperty UseBackgroundProperty = DependencyProperty.RegisterAttached(
            "UseBackground",
            typeof(bool),
            typeof(ComboboxEx),
            new PropertyMetadata(
                false,
                OnUseBackgroundChanged));

        /// <summary>Helper for getting <see cref="UseBackgroundProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to read <see cref="UseBackgroundProperty"/> from.</param>
        /// <returns>Value property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(ComboBox))]
        public static bool GetUseBackground(ComboBox element)
        {
            return (bool)element.GetValue(UseBackgroundProperty);
        }

        /// <summary>Helper for setting <see cref="UseBackgroundProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="UseBackgroundProperty"/> on.</param>
        /// <param name="value">UseBackground property value.</param>
        public static void SetUseBackground(ComboBox element, bool value)
        {
            element.SetValue(UseBackgroundProperty, value);
        }

        private static void OnUseBackgroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ComboBox comboBox)
            {
            }
        }
    }
}
