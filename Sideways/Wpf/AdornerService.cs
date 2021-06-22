namespace Sideways
{
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Threading;

    public static class AdornerService
    {
        private static readonly DependencyProperty AdornerLayerProperty = DependencyProperty.RegisterAttached(
            "AdornerLayer",
            typeof(AdornerLayer),
            typeof(AdornerService),
            new PropertyMetadata(default(AdornerLayer)));

        public static void Show(Adorner adorner)
        {
            if (adorner is null)
            {
                throw new System.ArgumentNullException(nameof(adorner));
            }

            Show(adorner, retry: true);
        }

        public static void Remove(Adorner adorner)
        {
            if (adorner is null)
            {
                throw new System.ArgumentNullException(nameof(adorner));
            }

            var adornerLayer = (AdornerLayer?)adorner.GetValue(AdornerLayerProperty) ??
                               AdornerLayer.GetAdornerLayer(adorner.AdornedElement);
            adornerLayer?.Remove(adorner);
            adorner.ClearValue(AdornerLayerProperty);
        }

        private static void Show(Adorner adorner, bool retry)
        {
            if (AdornerLayer.GetAdornerLayer(adorner.AdornedElement) is { } adornerLayer)
            {
                adornerLayer.Remove(adorner);
                adornerLayer.Add(adorner);
                adorner.SetCurrentValue(AdornerLayerProperty, adornerLayer);
            }
            else if (retry)
            {
                // try again later, perhaps giving layout a chance to create the adorner layer
#pragma warning disable VSTHRD001 // Avoid legacy thread switching APIs
                _ = adorner.Dispatcher.BeginInvoke(
#pragma warning restore VSTHRD001 // Avoid legacy thread switching APIs
                    DispatcherPriority.Loaded,
                    new DispatcherOperationCallback(Retry),
                    new object[] { adorner });
            }

            static object? Retry(object arg)
            {
                var args = (object[])arg;
                var adorner = (Adorner)args[0];
                Show(adorner, retry: false);
                return null;
            }
        }
    }
}
