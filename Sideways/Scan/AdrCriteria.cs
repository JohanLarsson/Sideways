namespace Sideways.Scan
{
    public sealed class AdrCriteria : Criteria
    {
        private float? min;
        private float? max;

        public override string Info => (this.Min, this.Max) switch
        {
            // ReSharper disable LocalVariableHidesMember
            (Min: { } min, Max: { } max) => $"ADR [{min:F1}..{max:F1}]",
            (Min: null, Max: { } max) => $"ADR [..{max:F1}]",
            (Min: { } min, Max: null) => $"ADR [{min:F1}..]",
            _ => "ADR *",
        };

        public override int ExtraDays => this.IsActive ? 20 : 0;

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

        public override bool IsSatisfied(SortedCandles candles, int index)
        {
            return (this.IsActive, this.Min, this.Max) switch
            {
                // ReSharper disable LocalVariableHidesMember
                (IsActive: true, Min: { } min, Max: { } max) => IsBetween(Adr(), min, max),
                (IsActive: true, Min: null, Max: { } max) => Adr() <= max,
                (IsActive: true, Min: { } min, Max: null) => Adr() >= min,
                _ => true,
            };

            static bool IsBetween(float adr, float min, float max) => adr >= min && adr <= max;

            float Adr() => candles.AsSpan()[^20..].Adr();
        }
    }
}
