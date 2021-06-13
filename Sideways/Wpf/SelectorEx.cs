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
            new PropertyMetadata(
                default(bool),
                OnRandomChanged));

        private static readonly DependencyProperty BackProperty = DependencyProperty.RegisterAttached(
            "Back",
            typeof(Stack),
            typeof(SelectorEx),
            new PropertyMetadata(default(Stack)));

        private static readonly DependencyProperty ForwardProperty = DependencyProperty.RegisterAttached(
            "Forward",
            typeof(Stack),
            typeof(SelectorEx),
            new PropertyMetadata(default(Stack)));

        private static readonly Random Random = new(DateTimeOffset.Now.Millisecond);

        static SelectorEx()
        {
            EventManager.RegisterClassHandler(typeof(Selector), UIElement.KeyDownEvent, new KeyEventHandler(OnKeyDown));
            EventManager.RegisterClassHandler(typeof(Selector), Selector.SelectionChangedEvent, new SelectionChangedEventHandler(OnSelectionChanged));

            static void OnKeyDown(object sender, KeyEventArgs e)
            {
                if (e.SystemKey == Key.R &&
                    sender is Selector selector &&
                    GetRandom(selector))
                {
                    selector.SelectedIndex = Random.Next(0, selector.Items.Count - 1);
                    if (selector is ListBox listBox)
                    {
                        listBox.ScrollIntoView(listBox.SelectedItem);
                    }

                    e.Handled = true;
                }
            }

            static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (sender is Selector selector &&
                    e.RemovedItems is { Count: 1 } removed)
                {
                    Stack.Get(selector, BackProperty).Push(removed[0]);
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

        private static void OnRandomChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true &&
                d is Selector selector)
            {
                selector.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseBack, OnBrowsBack, OnCanBrowseBack));
                selector.InputBindings.Add(new InputBinding(NavigationCommands.BrowseBack, new KeyGesture(Key.Left, ModifierKeys.Alt)));
                selector.CommandBindings.Add(new CommandBinding(NavigationCommands.BrowseForward, OnBrowseForward, OnCanBrowseForward));
                selector.InputBindings.Add(new InputBinding(NavigationCommands.BrowseForward, new KeyGesture(Key.Right, ModifierKeys.Alt)));

                static void OnCanBrowseBack(object sender, CanExecuteRoutedEventArgs e)
                {
                    e.CanExecute = ((Selector)sender).GetValue(BackProperty) is Stack { IsEmpty: false };
                    e.Handled = true;
                }

                static void OnBrowsBack(object sender, ExecutedRoutedEventArgs e)
                {
                    var selector = (Selector)sender;
                    if (Stack.Get(selector, BackProperty) is { IsEmpty: false } back)
                    {
                        var item = back.Pop();
                        Stack.Get(selector, ForwardProperty).Push(selector.SelectedItem);
                        selector.SelectedItem = item;
                        _ = back.Pop();
                        if (selector is ListBox listBox)
                        {
                            listBox.ScrollIntoView(listBox.SelectedItem);
                        }
                    }

                    e.Handled = true;
                }

                static void OnCanBrowseForward(object sender, CanExecuteRoutedEventArgs e)
                {
                    e.CanExecute = ((Selector)sender).GetValue(ForwardProperty) is Stack { IsEmpty: false };
                    e.Handled = true;
                }

                static void OnBrowseForward(object sender, ExecutedRoutedEventArgs e)
                {
                    var selector = (Selector)sender;
                    if (Stack.Get(selector, ForwardProperty) is { } forward)
                    {
                        var item = forward.Pop();
                        selector.SelectedItem = item;
                        if (selector is ListBox listBox)
                        {
                            listBox.ScrollIntoView(listBox.SelectedItem);
                        }
                    }

                    e.Handled = true;
                }
            }
        }

        private sealed class Stack
        {
            private readonly object?[] inner;
            private int index;
            private int count;

            private Stack(int size)
            {
                this.inner = new object[size];
            }

            internal bool IsEmpty => this.count == 0;

            internal static Stack Get(Selector selector, DependencyProperty property)
            {
                var stack = (Stack?)selector.GetValue(property);
                if (stack is null)
                {
                    stack = new Stack(100);
                    selector.SetCurrentValue(property, stack);
                }

                return stack;
            }

            internal void Push(object? item)
            {
                this.inner[this.index] = item;
                this.index = this.index == this.inner.Length - 1
                    ? 0
                    : this.index + 1;
                this.count = Math.Min(this.count + 1, this.inner.Length);
            }

            internal object? Pop()
            {
                this.index = this.index == 0
                    ? this.inner.Length - 1
                    : this.index - 1;
                this.count = Math.Max(this.count - 1, 0);
                return this.inner[this.index];
            }
        }
    }
}
