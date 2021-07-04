namespace Sideways.Scan
{
    public sealed class AdrCriteria : Criteria
    {
        private Percent? min = new Percent(5);
        private Percent? max;

        public override string Info => (this.min, this.max) switch
        {
            // ReSharper disable LocalVariableHidesMember
            (min: { } min, max: { } max) => $"{min.Scalar:#.#} ≤ ADR ≤ {max.Scalar:#.#}",
            (min: null, max: { } max) => $"ADR ≤ {max.Scalar:#.#}",
            (min: { } min, max: null) => $"{min.Scalar:#.#} ≤ ADR",
            _ => "ADR *",
            //// ReSharper restore LocalVariableHidesMember
        };

        public override int ExtraDays => this.IsActive ? 20 : 0;

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

            return candles.CanSlice(index, -20) &&
                   candles.Slice(index, -20).Adr().IsBetween(this.min ?? Percent.MinValue, this.max ?? Percent.MaxValue);
        }
    }
}
