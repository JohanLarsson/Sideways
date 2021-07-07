namespace Sideways
{
    using System.Windows;
    using System.Windows.Input;

    public static class Priority
    {
        public static readonly DependencyProperty OverProperty = DependencyProperty.RegisterAttached(
            "Over",
            typeof(UIElement),
            typeof(Priority),
            new PropertyMetadata(
                default(UIElement),
                OnOverChanged));

        private static readonly DependencyProperty OverridesProperty = DependencyProperty.RegisterAttached(
            "Overrides",
            typeof(InputBindingCollection),
            typeof(Priority),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.Inherits));

        static Priority()
        {
            EventManager.RegisterClassHandler(typeof(UIElement), UIElement.PreviewKeyDownEvent, new KeyEventHandler(OnPreviewKeyDown));

            static void OnPreviewKeyDown(object sender, KeyEventArgs e)
            {
                if (sender is UIElement element &&
                    element.GetValue(OverridesProperty) is InputBindingCollection overrides)
                {
                    foreach (InputBinding binding in overrides)
                    {
                        if (binding.Gesture.Matches(sender, e))
                        {
                            if (binding.Command is RoutedCommand routed)
                            {
                                routed.Execute(binding.CommandParameter, binding.CommandTarget);
                            }
                            else
                            {
                                binding.Command.Execute(binding.CommandParameter);
                            }

                            e.Handled = true;
                        }
                    }
                }
            }
        }

        [AttachedPropertyBrowsableForType(typeof(InputBinding))]
        public static UIElement? GetOver(InputBinding element) => (UIElement?)element.GetValue(OverProperty);

        public static void SetOver(InputBinding element, UIElement? value) => element.SetValue(OverProperty, value);

        private static void OnOverChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is UIElement element)
            {
                var overrides = (InputBindingCollection?)element.GetValue(OverridesProperty) ?? Create();
                overrides.Add((InputBinding)d);

                InputBindingCollection Create()
                {
                    var collection = new InputBindingCollection();
                    element.SetValue(OverridesProperty, collection);
                    return collection;
                }
            }
        }
    }
}
