namespace Sideways
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class AlphaVantageSettings : INotifyPropertyChanged
    {
        private AlphaVantageClientSettings clientSettings = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public AlphaVantageClientSettings ClientSettings
        {
            get => this.clientSettings;
            set
            {
                if (ReferenceEquals(value, this.clientSettings))
                {
                    return;
                }

                this.clientSettings = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
