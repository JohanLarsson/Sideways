namespace Sideways
{
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Windows.Input;

    public class SyncViewModel : INotifyPropertyChanged
    {
        private string? fileName;
        private string? status = "idle";

        public SyncViewModel()
        {
            this.OneWayFromFileCommand = new RelayCommand(
                _ =>
                {
                    var source = new FileInfo(this.fileName!);
                    foreach (var symbol in Database.ReadSymbols(source))
                    {
                        this.Status = $"copying {symbol}";
                        Sync.CopyAll(symbol, source, Database.DbFile);
                    }

                    this.Status = "done";
                },
                _ => this.fileName is { } file && File.Exists(file));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand OneWayFromFileCommand { get; }

        public string? FileName
        {
            get => this.fileName;
            set
            {
                if (value == this.fileName)
                {
                    return;
                }

                this.fileName = value;
                this.OnPropertyChanged();
            }
        }

        public string? Status
        {
            get => this.status;
            private set
            {
                if (value == this.status)
                {
                    return;
                }

                this.status = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
