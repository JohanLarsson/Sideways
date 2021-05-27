namespace Sideways
{
    using System.Windows;

    public static class Icon
    {
        public static readonly DependencyProperty IsSpinningProperty = DependencyProperty.RegisterAttached(
            "IsSpinning",
            typeof(bool),
            typeof(Icon),
            new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty IsPulsingProperty = DependencyProperty.RegisterAttached(
            "IsPulsing",
            typeof(bool),
            typeof(Icon),
            new PropertyMetadata(default(bool)));

        /// <summary>Helper for getting <see cref="IsSpinningProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to read <see cref="IsSpinningProperty"/> from.</param>
        /// <returns>Value property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetIsSpinning(DependencyObject element)
        {
            return (bool)element.GetValue(IsSpinningProperty);
        }

        /// <summary>Helper for setting <see cref="IsSpinningProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="IsSpinningProperty"/> on.</param>
        /// <param name="value">IsSpinning property value.</param>
        public static void SetIsSpinning(DependencyObject element, bool value)
        {
            element.SetValue(IsSpinningProperty, value);
        }

        /// <summary>Helper for getting <see cref="IsPulsingProperty"/> from <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to read <see cref="IsPulsingProperty"/> from.</param>
        /// <returns>Value property value.</returns>
        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetIsPulsing(DependencyObject element)
        {
            return (bool)element.GetValue(IsPulsingProperty);
        }

        /// <summary>Helper for setting <see cref="IsPulsingProperty"/> on <paramref name="element"/>.</summary>
        /// <param name="element"><see cref="DependencyObject"/> to set <see cref="IsPulsingProperty"/> on.</param>
        /// <param name="value">Pulse property value.</param>
        public static void SetIsPulsing(DependencyObject element, bool value)
        {
            element.SetValue(IsPulsingProperty, value);
        }
    }
}
