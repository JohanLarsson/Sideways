namespace Sideways.Scan
{
    public sealed class GapCriteria : Criteria
    {
        private Percent? min;
        private Percent? max;

        public override string Info => (this.min, this.max) switch
        {
            // ReSharper disable LocalVariableHidesMember
            (min: { } min, max: { } max) => $"{min:#.#} ≤ gap ≤ {max:#.#}",
            (min: null, max: { } max) => $"gap ≤ {max:#.#}",
            (min: { } min, max: null) => $"{min:#.#} ≤ gap",
            _ => "Gap *",
            //// ReSharper restore LocalVariableHidesMember
        };

        public override int ExtraDays => 1;

        public Percent? Min
        {
            get => this.min;
            set
            {
                if (value == this.min)
                {
                    return;
                }

                this.min = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Info));
            }
        }

        public Percent? Max
        {
            get => this.max;
            set
            {
                if (value == this.max)
                {
                    return;
                }

                this.max = value;
                this.OnPropertyChanged();
                this.OnPropertyChanged(nameof(this.Info));
            }
        }

        public bool IsSatisfied(SortedCandles candles, int index)
        {
            if (!this.IsActive)
            {
                return true;
            }

            if (index < 1)
            {
                return false;
            }

            return Percent.Change(candles[index].Open, candles[index - 1].Close).IsBetween(this.min ?? Percent.MinValue, this.max ?? Percent.MaxValue);
        }
    }
}
