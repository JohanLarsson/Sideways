namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;

    public static class SelectCommands
    {
        public static readonly RoutedCommand Random = new(nameof(Random), typeof(SelectCommands));
        public static readonly RoutedCommand Back = new(nameof(Back), typeof(SelectCommands));
        public static readonly RoutedCommand Forward = new(nameof(Forward), typeof(SelectCommands));

        public static readonly DependencyProperty IsEnabledProperty = DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(SelectCommands),
            new FrameworkPropertyMetadata(
                default(bool),
                FrameworkPropertyMetadataOptions.Inherits));

        private static readonly DependencyProperty JournalProperty = DependencyProperty.RegisterAttached("Journal", typeof(Journal), typeof(SelectCommands));

        private static readonly Random RandomIndex = new(DateTimeOffset.Now.Millisecond);

        static SelectCommands()
        {
            CommandManager.RegisterClassCommandBinding(typeof(Selector), new CommandBinding(Random, OnSelectRandom, OnCanSelectRandom));
            CommandManager.RegisterClassInputBinding(typeof(Selector), new InputBinding(Random, new KeyGesture(Key.R, ModifierKeys.Alt)));

            CommandManager.RegisterClassCommandBinding(typeof(Selector), new CommandBinding(Back, OnSelectBack, OnCanSelectBack));
            CommandManager.RegisterClassInputBinding(typeof(Selector), new InputBinding(Back, new KeyGesture(Key.Left, ModifierKeys.Alt)));

            CommandManager.RegisterClassCommandBinding(typeof(Selector), new CommandBinding(Forward, OnSelectForward, OnCanSelectForward));
            CommandManager.RegisterClassInputBinding(typeof(Selector), new InputBinding(Forward, new KeyGesture(Key.Right, ModifierKeys.Alt)));

            EventManager.RegisterClassHandler(typeof(Selector), Selector.SelectionChangedEvent, new SelectionChangedEventHandler(OnSelectionChanged));
            static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
            {
                if (sender is Selector selector &&
                    GetIsEnabled(selector) &&
                    e.RemovedItems is { Count: 1 } removed)
                {
                    var journal = (Journal?)selector.GetValue(JournalProperty);
                    if (journal is null)
                    {
                        journal = new();
                        selector.SetCurrentValue(JournalProperty, journal);
                    }

                    journal.Back.Push(removed[0]);
                }
            }

            static void OnCanSelectRandom(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = sender is Selector { Items: { Count: > 0 }, IsEnabled: true } selector &&
                               GetIsEnabled(selector);
                e.Handled = true;
            }

            static void OnSelectRandom(object sender, ExecutedRoutedEventArgs e)
            {
                if (sender is Selector { Items: { Count: > 0 }, IsEnabled: true } selector &&
                    GetIsEnabled(selector))
                {
                    selector.SelectedIndex = RandomIndex.Next(0, selector.Items.Count - 1);
                    if (selector is ListBox listBox)
                    {
                        listBox.ScrollIntoView(listBox.SelectedItem);
                    }

                    e.Handled = true;
                }
            }

            static void OnCanSelectBack(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = ((Selector)sender).GetValue(JournalProperty) is Journal { Back: { IsEmpty: false } };
                e.Handled = true;
            }

            static void OnSelectBack(object sender, ExecutedRoutedEventArgs e)
            {
                if (sender is Selector selector &&
                    selector.GetValue(JournalProperty) is Journal { Back: { IsEmpty: false } back, Forward: { } forward })
                {
                    var item = back.Pop();
                    forward.Push(selector.SelectedItem);
                    selector.SelectedItem = item;
                    _ = back.Pop();
                    if (selector is ListBox listBox)
                    {
                        listBox.ScrollIntoView(listBox.SelectedItem);
                    }

                    e.Handled = true;
                }
            }

            static void OnCanSelectForward(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = ((Selector)sender).GetValue(JournalProperty) is Journal { Forward: { IsEmpty: false } };
                e.Handled = true;
            }

            static void OnSelectForward(object sender, ExecutedRoutedEventArgs e)
            {
                if (sender is Selector selector &&
                    selector.GetValue(JournalProperty) is Journal { Forward: { IsEmpty: false } forward })
                {
                    var item = forward.Pop();
                    selector.SelectedItem = item;
                    if (selector is ListBox listBox)
                    {
                        listBox.ScrollIntoView(listBox.SelectedItem);
                    }

                    e.Handled = true;
                }
            }
        }

        [AttachedPropertyBrowsableForType(typeof(DependencyObject))]
        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);

        private class Journal
        {
            internal readonly Stack Back = new(100);
            internal readonly Stack Forward = new(100);
        }

        private sealed class Stack
        {
            private readonly object?[] inner;
            private int index;
            private int count;

            internal Stack(int size)
            {
                this.inner = new object[size];
            }

            internal bool IsEmpty => this.count == 0;

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
