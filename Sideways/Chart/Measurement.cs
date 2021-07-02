namespace Sideways
{
    public class Measurement
    {
        private Measurement(TimeAndPrice @from, TimeAndPrice? to, int? candles, Percent? adr, float? atr)
        {
            this.From = @from;
            this.To = to;
            this.Candles = candles;
            this.Adr = adr;
            this.Atr = atr;
        }

        public TimeAndPrice From { get; }

        public TimeAndPrice? To { get; }

        public int? Candles { get; }

        public Percent? Adr { get; }

        public float? Atr { get; }

        public Percent? PercentChange => this.To is { Price: var to } ? Percent.Change(this.From.Price, to) : null;

        public float? DollarChange => this.To?.Price - this.From.Price;

        public float? AdrChange => this.PercentChange?.Scalar / this.Adr?.Scalar;

        public float? AtrChange => this.DollarChange / this.Atr;

        public string? TimeChange => (this.To?.Time - this.From.Time) switch
        {
            { TotalHours: < 1, Minutes: var minutes } => $"{minutes} m",
            { TotalDays: < 1, Hours: var hours, Minutes: var minutes } => $"{hours} h {minutes} m",
            { Days: var days, Hours: var hours, Minutes: var minutes }
                when hours > 0 || minutes > 0
                => $"{days} d {hours} h {minutes} m",
            { Days: var days } => $"{days} d",
            _ => string.Empty,
        };

        public static Measurement Start(TimeAndPrice timeAndPrice) => new(timeAndPrice, null, null, null, null);

        public Measurement WithEnd(TimeAndPrice timeAndPrice, int candles, Percent? adr, float? atr) => new(this.From, timeAndPrice, candles, adr, atr);
    }
}
