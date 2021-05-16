namespace Sideways
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class Simulation : INotifyPropertyChanged
    {
        private readonly ObservableCollection<Position> positions = new();
        private readonly ObservableCollection<Trade> trades = new();

        private string name = $"Simulation_{DateTime.Now.ToLongDateString()}";
        private decimal balance;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Simulation()
        {
            this.balance = 100_000;
            this.Positions = new ReadOnlyObservableCollection<Position>(this.positions);
            this.Trades = new ReadOnlyObservableCollection<Trade>(this.trades);
        }

        public ReadOnlyObservableCollection<Position> Positions { get; }

        public ReadOnlyObservableCollection<Trade> Trades { get; }

        public string Name
        {
            get => this.name;
            set
            {
                if (value == this.name)
                {
                    return;
                }

                this.name = value;
                this.OnPropertyChanged();
            }
        }

        public decimal Balance
        {
            get => this.balance;
            private set
            {
                if (value == this.balance)
                {
                    return;
                }

                this.balance = value;
                this.OnPropertyChanged();
            }
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
