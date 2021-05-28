namespace Sideways
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class AlphaVantageClientSettings : INotifyPropertyChanged
    {
        private string? apiKey;
        private int maxCallsPerMinute = 5;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string? ApiKey
        {
            get => this.apiKey;
            set
            {
                if (value == this.apiKey)
                {
                    return;
                }

                this.apiKey = value;
                this.OnPropertyChanged();
            }
        }

        public int MaxCallsPerMinute
        {
            get => this.maxCallsPerMinute;
            set
            {
                if (value == this.maxCallsPerMinute)
                {
                    return;
                }

                this.maxCallsPerMinute = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
