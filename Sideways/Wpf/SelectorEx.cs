namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    public static class SelectorEx
    {
        public static readonly DependencyProperty RandomProperty = DependencyProperty.RegisterAttached(
            "Random",
            typeof(bool),
            typeof(SelectorEx),
            new PropertyMetadata(default(bool)));

        private static readonly Random Random = new(DateTimeOffset.Now.Millisecond);

        static SelectorEx()
        {
            EventManager.RegisterClassHandler(typeof(Selector), UIElement.KeyDownEvent, new KeyEventHandler(OnKeyDown));

            static void OnKeyDown(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.R &&
                    Keyboard.Modifiers == ModifierKeys.Control &&
                    sender is Selector selector &&
                    GetRandom(selector))
                {
                    selector.SelectedIndex = Random.Next(0, selector.Items.Count - 1);
                    if (selector is ListBox listBox)
                    {
                        listBox.ScrollIntoView(listBox.SelectedItem);
                    }
                }
            }
        }

        [AttachedPropertyBrowsableForType(typeof(Selector))]
        public static bool GetRandom(Selector element)
        {
            return (bool)element.GetValue(RandomProperty);
        }

        public static void SetRandom(Selector element, bool value)
        {
            element.SetValue(RandomProperty, value);
        }
    }
}
