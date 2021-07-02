namespace Sideways.Scan
{
    public sealed class AtrCriteria : Criteria
    {
        private float? min;
        private float? max;

        public override string Info => (this.Min, this.Max) switch
        {
            // ReSharper disable LocalVariableHidesMember
            (Min: { } min, Max: { } max) => $"ATR [{min:F1}..{max:F1}]",
            (Min: null, Max: { } max) => $"ATR [..{max:F1}]",
            (Min: { } min, Max: null) => $"ATR [{min:F1}..]",
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
            return !this.IsActive ||
                   new FloatRange(this.min ?? float.MinValue, this.max ?? float.MaxValue).Contains(candles.Slice(index, -21).Atr());
        }
    }
}
