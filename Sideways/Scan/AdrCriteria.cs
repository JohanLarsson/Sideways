namespace Sideways.Scan
{
    public sealed class AdrCriteria : Criteria
    {
        private Percent? min;
        private Percent? max;

        public override string Info => (this.Min, this.Max) switch
        {
            // ReSharper disable LocalVariableHidesMember
            (Min: { } min, Max: { } max) => $"ADR [{min:F1}..{max:F1}]",
            (Min: null, Max: { } max) => $"ADR [..{max:F1}]",
            (Min: { } min, Max: null) => $"ADR [{min:F1}..]",
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

        public override bool IsSatisfied(SortedCandles candles, int index)
        {
            return !this.IsActive ||
                   candles.AsSpan()[^20..].Adr().IsBetween(this.min ?? Percent.MinValue, this.max ?? Percent.MaxValue);
        }
    }
}
