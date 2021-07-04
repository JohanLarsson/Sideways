namespace Sideways.Scan
{
    public sealed class AtrCriteria : Criteria
    {
        private float? min;
        private float? max;

        public override string Info => (this.min, this.max) switch
        {
            // ReSharper disable LocalVariableHidesMember
            (min: { } min, max: { } max) => $"{min:#.#} ≤ ATR ≤ {max:#.#}",
            (min: null, max: { } max) => $"ATR ≤ {max:#.#}",
            (min: { } min, max: null) => $"{min:#.#} ≤ ATR",
            _ => "ATR *",
            //// ReSharper restore LocalVariableHidesMember
        };

        public override int ExtraDays => this.IsActive ? 21 : 0;

        public float? Min
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

        public float? Max
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

            return candles.CanSlice(index, -21) &&
                   new FloatRange(this.min ?? float.MinValue, this.max ?? float.MaxValue).Contains(candles.Slice(index, -21).Atr());
        }
    }
}
