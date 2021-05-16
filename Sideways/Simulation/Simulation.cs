namespace Sideways
{
    using System;
    using System.Collections.Immutable;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;

    public class Simulation : INotifyPropertyChanged
    {
        private string name = $"Simulation_{DateTime.Now.ToLongDateString()}";
        private decimal balance;
        private ImmutableList<Position> positions = ImmutableList<Position>.Empty;
        private ImmutableList<Trade> trades = ImmutableList<Trade>.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Simulation()
        {
            this.balance = 100_000;
        }

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

        public ImmutableList<Position> Positions
        {
            get => this.positions;
            private set
            {
                if (ReferenceEquals(value, this.positions))
                {
                    return;
                }

                this.positions = value;
                this.OnPropertyChanged();
            }
        }

        public ImmutableList<Trade> Trades
        {
            get => this.trades;
            private set
            {
                if (ReferenceEquals(value, this.trades))
                {
                    return;
                }

                this.trades = value;
                this.OnPropertyChanged();
            }
        }

        public void Buy(string symbol, float price, int shares, DateTimeOffset time)
        {
            if (this.Balance < (decimal)(price * shares))
            {
                throw new InvalidOperationException("Not enough money.");
            }

            this.Balance -= (decimal)price * shares;
            var buys = this.positions.SingleOrDefault(x => x.Symbol == symbol)?.Buys ?? ImmutableList<Buy>.Empty;
            this.Positions = this.Positions.Add(new Position(symbol, buys.Add(new Buy(shares, time, (decimal)price))));
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
