namespace Sideways
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    public static class Navigate
    {
        public static readonly DependencyProperty TargetProperty = DependencyProperty.RegisterAttached(
            "Target",
            typeof(ContentPresenter),
            typeof(Navigate),
            new PropertyMetadata(default(ContentPresenter)));

        public static readonly DependencyProperty TemplateProperty = DependencyProperty.RegisterAttached(
            "Template",
            typeof(DataTemplate),
            typeof(Navigate),
            new PropertyMetadata(default(DataTemplate)));

        static Navigate()
        {
            EventManager.RegisterClassHandler(typeof(RadioButton), UIElement.PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(OnPreviewMouseLeftButtonDown));

            static void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
            {
                if (sender is RadioButton radioButton &&
                    GetTarget(radioButton) is { } target &&
                    GetTemplate(radioButton) is { } template)
                {
                    target.ContentTemplate = template;
                }
            }
        }

        [AttachedPropertyBrowsableForType(typeof(RadioButton))]
        public static ContentPresenter? GetTarget(RadioButton element)
        {
            return (ContentPresenter)element.GetValue(TargetProperty);
        }

        public static void SetTarget(RadioButton element, ContentPresenter? value)
        {
            element.SetValue(TargetProperty, value);
        }

        [AttachedPropertyBrowsableForType(typeof(RadioButton))]
        public static DataTemplate? GetTemplate(RadioButton element)
        {
            return (DataTemplate)element.GetValue(TemplateProperty);
        }

        public static void SetTemplate(RadioButton element, DataTemplate? value)
        {
            element.SetValue(TemplateProperty, value);
        }
    }
}
