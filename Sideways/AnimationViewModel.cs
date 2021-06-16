namespace Sideways
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Windows.Input;

    public sealed class AnimationViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly MainViewModel main;

        private Timer? timer;
        private bool disposed;

        public AnimationViewModel(MainViewModel main)
        {
            this.main = main;
            this.StartCommand = new RelayCommand(_ => this.Start());

            this.StopCommand = new RelayCommand(_ => this.Stop());

            this.ToggleCommand = new RelayCommand(_ =>
            {
                if (this.IsRunning)
                {
                    this.Stop();
                }
                else
                {
                    this.Start();
                }
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand StartCommand { get; }

        public ICommand StopCommand { get; }

        public ICommand ToggleCommand { get; }

        public bool IsRunning => this.timer is { };

        public void Dispose()
        {
            if (this.disposed)
            {
                return;
            }

            this.disposed = true;
            this.timer?.Dispose();
        }

        private void Start()
        {
            this.timer?.Dispose();
            this.timer = new Timer(OnTick, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(50));
            this.OnPropertyChanged(nameof(this.IsRunning));
            void OnTick(object? state)
            {
                if (this.main is { CurrentSymbol: { Candles: { } candles } })
                {
                    this.main.Time = candles.Skip(this.main.Time, CandleInterval.Minute, 1);
                }
            }
        }

        private void Stop()
        {
            this.timer?.Dispose();
            this.timer = null;
            this.OnPropertyChanged(nameof(this.IsRunning));
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
