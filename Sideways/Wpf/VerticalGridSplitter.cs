namespace Sideways
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public static class VerticalGridSplitter
    {
        public static readonly RoutedCommand TogglePreviousCommand = new(nameof(TogglePreviousCommand), typeof(VerticalGridSplitter));
        public static readonly RoutedCommand ToggleNextCommand = new(nameof(ToggleNextCommand), typeof(VerticalGridSplitter));

        public static readonly DependencyProperty IsToggleEnabledProperty = DependencyProperty.RegisterAttached(
            "IsToggleEnabled",
            typeof(bool),
            typeof(VerticalGridSplitter),
            new PropertyMetadata(
                false,
                OnIsToggleEnabledChanged));

        private static readonly DependencyPropertyKey PreviousDefinitionPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "PreviousDefinition",
            typeof(ColumnDefinition),
            typeof(VerticalGridSplitter),
            new PropertyMetadata(default(ColumnDefinition)));

        public static readonly DependencyProperty PreviousDefinitionProperty = PreviousDefinitionPropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey NextDefinitionPropertyKey = DependencyProperty.RegisterAttachedReadOnly(
            "NextDefinition",
            typeof(ColumnDefinition),
            typeof(VerticalGridSplitter),
            new PropertyMetadata(default(ColumnDefinition)));

        public static readonly DependencyProperty NextDefinitionProperty = NextDefinitionPropertyKey.DependencyProperty;

        private static readonly DependencyProperty OriginalDefinitionsProperty = DependencyProperty.RegisterAttached(
            "OriginalDefinitions",
            typeof(OriginalDefinitions),
            typeof(VerticalGridSplitter));

        static VerticalGridSplitter()
        {
            CommandManager.RegisterClassCommandBinding(typeof(GridSplitter), new CommandBinding(TogglePreviousCommand, OnTogglePrevious, OnCanReset));
            CommandManager.RegisterClassCommandBinding(typeof(GridSplitter), new CommandBinding(ToggleNextCommand, OnToggleNext, OnCanReset));

            static void OnCanReset(object sender, CanExecuteRoutedEventArgs e)
            {
                e.CanExecute = sender is GridSplitter { IsEnabled: true } splitter &&
                               splitter.GetValue(OriginalDefinitionsProperty) is OriginalDefinitions;
                e.Handled = true;
            }

            static void OnTogglePrevious(object sender, ExecutedRoutedEventArgs e)
            {
                var splitter = (GridSplitter)sender;
                if (splitter.GetValue(OriginalDefinitionsProperty) is OriginalDefinitions original)
                {
                    if (original.CanReset())
                    {
                        original.Reset();
                        e.Handled = true;
                    }
                    else if (GetPreviousDefinition(splitter) is { } definition)
                    {
                        definition.Width = new GridLength(0, GridUnitType.Pixel);
                    }
                }
            }

            static void OnToggleNext(object sender, ExecutedRoutedEventArgs e)
            {
                var splitter = (GridSplitter)sender;
                if (splitter.GetValue(OriginalDefinitionsProperty) is OriginalDefinitions original)
                {
                    if (original.CanReset())
                    {
                        original.Reset();
                        e.Handled = true;
                    }
                    else if (GetNextDefinition(splitter) is { } definition)
                    {
                        definition.Width = new GridLength(0, GridUnitType.Pixel);
                    }
                }
            }
        }

        [AttachedPropertyBrowsableForType(typeof(GridSplitter))]
        public static bool GetIsToggleEnabled(GridSplitter element) => (bool)element.GetValue(IsToggleEnabledProperty);

        public static void SetIsToggleEnabled(GridSplitter element, bool value) => element.SetValue(IsToggleEnabledProperty, value);

        [AttachedPropertyBrowsableForType(typeof(GridSplitter))]
        public static ColumnDefinition? GetPreviousDefinition(GridSplitter element) => (ColumnDefinition)element.GetValue(PreviousDefinitionProperty);

        public static void SetPreviousDefinition(GridSplitter element, ColumnDefinition? value) => element.SetValue(PreviousDefinitionPropertyKey, value);

        [AttachedPropertyBrowsableForType(typeof(GridSplitter))]
        public static ColumnDefinition? GetNextDefinition(GridSplitter element) => (ColumnDefinition)element.GetValue(NextDefinitionProperty);

        public static void SetNextDefinition(GridSplitter element, ColumnDefinition? value) => element.SetValue(NextDefinitionPropertyKey, value);

        private static void OnIsToggleEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true &&
                d is GridSplitter { Parent: Grid grid } splitter)
            {
                var column = Grid.GetColumn(splitter);
                splitter.SetValue(OriginalDefinitionsProperty, new OriginalDefinitions(column, grid.ColumnDefinitions));
                SetPreviousDefinition(splitter, grid.ColumnDefinitions[column - 1]);
                SetNextDefinition(splitter, grid.ColumnDefinitions[column + 1]);
            }
        }

        private class OriginalDefinitions
        {
            private readonly ColumnDefinition previous;
            private readonly GridLength previousLength;
            private readonly ColumnDefinition next;
            private readonly GridLength nextLength;

            internal OriginalDefinitions(int column, ColumnDefinitionCollection definitions)
            {
                switch (definitions[column - 1], definitions[column + 1])
                {
                    case ({ Width: { IsStar: true } } p, { Width: { IsStar: true } } n):
                        this.previous = p;
                        this.previousLength = p.Width;
                        this.next = n;
                        this.nextLength = n.Width;
                        break;
                    case ({ Width: { IsStar: true } } p, { Width: { IsAuto: true } } n):
                        this.previous = p;
                        this.previousLength = p.Width;
                        this.next = n;
                        this.nextLength = n.Width;
                        break;
                    default:
                        throw new NotSupportedException("Only supporting a subset for now.");
                }
            }

            internal bool CanReset()
            {
                return (this.previousLength, this.nextLength) switch
                {
                    ({ IsStar: true }, { IsStar: true }) => Math.Abs((this.previousLength.Value / this.nextLength.Value) - (this.previous.ActualWidth / this.next.ActualWidth)) > 0.01,
                    ({ IsStar: true }, { IsAuto: true }) => !this.next.Width.IsAuto,
                    _ => throw new NotSupportedException("Only supporting a subset for now."),
                };
            }

            internal void Reset()
            {
                switch (this.previousLength, this.nextLength)
                {
                    case ({ IsStar: true }, { IsStar: true }):
                        var sum = this.previous.ActualWidth + this.next.ActualWidth;
                        var originalSum = this.previousLength.Value + this.nextLength.Value;
                        this.previous.SetCurrentValue(ColumnDefinition.WidthProperty, new GridLength(this.previousLength.Value / originalSum * sum, GridUnitType.Star));
                        this.next.SetCurrentValue(ColumnDefinition.WidthProperty, new GridLength(this.nextLength.Value / originalSum * sum, GridUnitType.Star));
                        break;
                    case ({ IsStar: true }, { IsAuto: true }):
                        this.next.SetCurrentValue(ColumnDefinition.WidthProperty, GridLength.Auto);
                        break;
                }
            }
        }
    }
}
