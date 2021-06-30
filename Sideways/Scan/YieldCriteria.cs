namespace Sideways.Scan
{
    public sealed class YieldCriteria : Criteria
    {
        private int days;
        private float min;

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

        public float Min
        {
            get => this.min;
            set
            {
                if (value.Equals(this.min))
                {
                    return;
                }

                this.min = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Info));
            }
        }

        public override int ExtraDays => this.Days;

        public override string Info => $"Min {this.min}% in {this.days} days";

        public override bool IsSatisfied(SortedCandles candles, int index)
        {
            return (this.IsActive, this.days, this.min) switch
            {
                // ReSharper disable LocalVariableHidesMember
                (IsActive: true, days: > 0 and var days, min: > 0 and var min) => candles[index].Close / candles[index - days].Open > min,
                _ => true,
            };
        }
    }
}
