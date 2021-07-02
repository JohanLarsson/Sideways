namespace Sideways.Scan
{
    public sealed class PriceCriteria : Criteria
    {
        private float? min = 1;
        private float? max;

        public override string Info => (this.min, this.max) switch
        {
            // ReSharper disable LocalVariableHidesMember
            (min: { } min, max: { } max) => $"{min:#.#} ≤ c ≤ {max:#.#}",
            (min: null, max: { } max) => $"c ≤ {max:#.#}",
            (min: { } min, max: null) => $"{min:#.#} ≤ c",
            _ => "Price *",
            //// ReSharper restore LocalVariableHidesMember
        };

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
            return !this.IsActive ||
                   new FloatRange(this.min ?? float.MinValue, this.max ?? float.MaxValue).Contains(candles[index].Close);
        }
    }
}
