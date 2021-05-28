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

        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetIsSpinning(DependencyObject element)
        {
            return (bool)element.GetValue(IsSpinningProperty);
        }

        public static void SetIsSpinning(DependencyObject element, bool value)
        {
            element.SetValue(IsSpinningProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetIsPulsing(DependencyObject element)
        {
            return (bool)element.GetValue(IsPulsingProperty);
        }

        public static void SetIsPulsing(DependencyObject element, bool value)
        {
            element.SetValue(IsPulsingProperty, value);
        }
    }
}
