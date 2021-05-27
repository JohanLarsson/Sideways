namespace Sideways.AlphaVantage
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class DownloadState : INotifyPropertyChanged
    {
        private DateTimeOffset? start;
        private DateTimeOffset? end;
        private Exception? exception;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TimeSpan? Duration => (this.end ?? DateTimeOffset.Now) - this.start;

        public DownloadStatus Status => this switch
        {
            { Start: null } => DownloadStatus.Waiting,
            { Start: { }, End: null, Exception: null } => DownloadStatus.Running,
            { Start: { }, End: { }, Exception: null } => DownloadStatus.Completed,
            { Exception: { } } => DownloadStatus.Error,
            _ => DownloadStatus.Unknown,
        };

        public DateTimeOffset? Start
        {
            get => this.start;
            set
            {
                if (value == this.start)
                {
                    return;
                }

                this.start = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Duration));
                this.OnPropertyChanged(nameof(this.Status));
            }
        }

        public DateTimeOffset? End
        {
            get => this.end;
            set
            {
                if (value == this.end)
                {
                    return;
                }

                this.end = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Duration));
                this.OnPropertyChanged(nameof(this.Status));
            }
        }

        public Exception? Exception
        {
            get => this.exception;
            set
            {
                if (ReferenceEquals(value, this.exception))
                {
                    return;
                }

                this.exception = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Status));
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
