namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class SyncViewModel : INotifyPropertyChanged
    {
        private string? fileName;
        private string? status = "idle";

        public SyncViewModel()
        {
            this.OneWayFromFileCommand = new RelayCommand(
#pragma warning disable VSTHRD101 // Avoid unsupported async delegates
                async _ =>
                {
                    try
                    {
                        this.Status = "reading symbols";
                        var source = new FileInfo(this.fileName!);
                        var symbols = await Task.Run(() => Database.ReadSymbols(source)).ConfigureAwait(false);
                        foreach (var symbol in symbols)
                        {
                            this.Status = $"copying {symbol}";
                            Sync.CopyAll(symbol, source, Database.DbFile);
                        }

                        this.Status = "done";
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception e)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        this.Status = e.Message;
                    }
                },
#pragma warning restore VSTHRD101 // Avoid unsupported async delegates
                _ => this.fileName is { } file &&
                             File.Exists(file) &&
                             this.status?.StartsWith("copying", StringComparison.Ordinal) is false &&
                             this.status != "reading symbols");
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
