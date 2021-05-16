namespace Sideways
{
    using System;
    using System.Collections.Generic;
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
            var buy = new Buy(shares, time, (decimal)price);
            if (this.positions.SingleOrDefault(x => x.Symbol == symbol) is { } existing)
            {
                this.Positions = this.positions.Replace(existing, new Position(symbol, existing.Buys.Add(buy)));
            }
            else
            {
                this.Positions = this.Positions.Add(new Position(symbol, ImmutableList.Create(buy)));
            }
        }

        public void Sell(string symbol, float price, int shares, DateTimeOffset time)
        {
            var position = this.positions.SingleOrDefault(x => x.Symbol == symbol);
            if (position is null)
            {
                throw new InvalidOperationException("Can't sell what you don't own.");
            }

            if (position.Buys.Sum(x => x.Shares) == shares)
            {
                this.Positions = this.positions.Remove(position);
                this.Trades = this.trades.Add(new Trade(symbol, position.Buys, new Sell(shares, time, (decimal)price)));
            }
            else
            {
                this.Positions = this.positions.Replace(position, new Position(symbol, Remaining()));
                this.Trades = this.trades.Add(new Trade(symbol, Sold().ToImmutableList(), new Sell(shares, time, (decimal)price)));

                ImmutableList<Buy> Remaining()
                {
                    var buys = position.Buys;
                    var toSell = shares;
                    while (toSell != 0)
                    {
                        var last = buys[^1];
                        if (last.Shares <= toSell)
                        {
                            toSell -= last.Shares;
                            buys = buys.RemoveAt(buys.Count - 1);
                        }
                        else
                        {
                            buys = buys.Replace(last, new Buy(last.Shares - toSell, last.Time, last.Price));
                            toSell = 0;
                        }
                    }

                    return buys;
                }

                IEnumerable<Buy> Sold()
                {
                    var buys = position.Buys;
                    var toSell = shares;
                    while (toSell != 0)
                    {
                        var last = buys[^1];
                        if (last.Shares <= toSell)
                        {
                            toSell -= last.Shares;
                            yield return last;
                        }
                        else
                        {
                            yield return new Buy(toSell, last.Time, last.Price);
                            toSell = 0;
                        }
                    }
                }
            }

            this.Balance += (decimal)(price * shares);
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
