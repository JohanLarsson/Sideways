namespace Sideways.Scan
{
    public class MinYield : Filter
    {
        private int days;
        private float @yield;

        public int Days
        {
            get => this.days;
            set
            {
                if (value == this.days)
                {
                    return;
                }

                this.days = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.ExtraDays));
                this.OnPropertyChanged(nameof(this.Info));
            }
        }

        public float Yield
        {
            get => this.yield;
            set
            {
                if (value == this.yield)
                {
                    return;
                }

                this.yield = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Info));
            }
        }

        public override int ExtraDays => this.Days;

        public override string Info => $"Min {this.yield}% in {this.days} days";

        public bool IsMatch(SortedCandles candles, int index) => candles[index].Close / candles[index - this.days].Close > this.yield;
    }
}
