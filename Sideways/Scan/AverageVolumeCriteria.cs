namespace Sideways.Scan
{
    public sealed class AverageVolumeCriteria : Criteria
    {
        private float? min;
        private float? max;

        public override string Info => (this.Min, this.Max) switch
        {
            // ReSharper disable LocalVariableHidesMember
            (Min: { } min, Max:
            { } max) => $"AV [{MillionConverter.DisplayText(min)}..{MillionConverter.DisplayText(max)}]",
            (Min: null, Max: { } max) => $"AV [..{MillionConverter.DisplayText(max)}]",
            (Min: { } min, Max: null) => $"AV [{MillionConverter.DisplayText(min)}..]",
            _ => "AV *",
            //// ReSharper restore LocalVariableHidesMember
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

        public bool IsSatisfied(SortedCandles candles, int index)
        {
            return !this.IsActive ||
                   new FloatRange(this.min ?? float.MinValue, this.max ?? float.MaxValue).Contains(candles.AsSpan().Slice(index, -20).Average(x => x.Volume));
        }
    }
}
