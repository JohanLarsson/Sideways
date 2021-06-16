namespace Sideways
{
    using System.Windows;

    public static class MessageBox
    {
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage messageBoxImage = MessageBoxImage.None)
        {
            if (Application.Current.MainWindow is { } window)
            {
                return System.Windows.MessageBox.Show(window, messageBoxText, caption, button, messageBoxImage);
            }

            return System.Windows.MessageBox.Show(messageBoxText, caption, button, messageBoxImage);
        }
    }
}
