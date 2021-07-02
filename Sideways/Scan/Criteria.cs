namespace Sideways.Scan
{
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public abstract class Criteria : INotifyPropertyChanged
    {
        private bool isActive;

        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual int ExtraDays { get; }

        public abstract string Info { get; }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (value == this.isActive)
                {
                    return;
                }

                this.isActive = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
