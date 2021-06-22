namespace Sideways
{
    using System.Globalization;

    public class Measurement
    {
        private Measurement(TimeAndPrice @from, TimeAndPrice? to, int? candles)
        {
            this.From = @from;
            this.To = to;
            this.Candles = candles;
        }

        public TimeAndPrice From { get; }

        public TimeAndPrice? To { get; }

        public int? Candles { get; }

        public double? PercentChange => 100 * (this.To?.Price - this.From.Price) / this.From.Price;

        public double? DollarChange => this.To?.Price - this.From.Price;

        public string? TimeChange => (this.To?.Time - this.From.Time) switch
        {
            { TotalHours: < 1, Minutes: var minutes } => $"{minutes} m",
            { TotalDays: < 1, Hours: var hours, Minutes: var minutes } => $"{hours} h {minutes} m",
            { TotalDays: var days } => $"{days} d",
        };

        public static Measurement Start(TimeAndPrice timeAndPrice) => new(timeAndPrice, null, null);

        public Measurement WithEnd(TimeAndPrice timeAndPrice, int candles) => new(this.From, timeAndPrice, candles);
    }
}
