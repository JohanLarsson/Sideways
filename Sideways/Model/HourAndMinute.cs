namespace Sideways
{
    using System;

    public readonly struct HourAndMinute : IEquatable<HourAndMinute>
    {
        public readonly int Hour;
        public readonly int Minute;

        public HourAndMinute(int hour, int minute)
        {
            this.Hour = hour;
            this.Minute = minute;
        }

        public static bool operator ==(HourAndMinute left, HourAndMinute right) => left.Equals(right);

        public static bool operator !=(HourAndMinute left, HourAndMinute right) => !left.Equals(right);

        public static HourAndMinute EndOfHourCandle(DateTimeOffset t) => t switch
        {
            { Hour: 9, Minute: 00 } => new(9, 00),
            { Hour: 9, Minute: <= 30 } => new(9, 30),
            { Hour: 9, Minute: > 30 } => new(10, 0),
            { Minute: 0 } => new(t.Hour, 0),
            _ => new(t.Hour + 1, 0),
        };

        public bool Equals(HourAndMinute other) => this.Hour == other.Hour && this.Minute == other.Minute;

        public override bool Equals(object? obj) => obj is HourAndMinute other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.Hour, this.Minute);
    }
}
