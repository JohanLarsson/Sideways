namespace Sideways
{
    using System.Windows.Media;

    public static class VisualExtensions
    {
        public static T? FirstAncestor<T>(this Visual child)
            where T : Visual
        {
            var parent = VisualTreeHelper.GetParent(child);
            while (parent is { })
            {
                if (parent is T match)
                {
                    return match;
                }

                parent = VisualTreeHelper.GetParent(parent);
            }

            return null;
        }
    }
}
