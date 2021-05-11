namespace Sideways
{
    using System;
    using System.Diagnostics;

    [DebuggerDisplay("{" + nameof(ToString) + "(),nq}")]
    public readonly struct Split : IEquatable<Split>
    {
        public Split(DateTimeOffset date, double coefficient)
        {
            this.Date = date;
            this.Coefficient = coefficient;
        }

        public DateTimeOffset Date { get; }

        public double Coefficient { get; }

        public static bool operator ==(Split left, Split right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Split left, Split right)
        {
            return !left.Equals(right);
        }

        public override string ToString() => $"{this.Date:yyyy’-‘MM’-‘dd} Coefficient: {this.Coefficient}";

        public bool Equals(Split other)
        {
            return this.Date.Equals(other.Date) && this.Coefficient.Equals(other.Coefficient);
        }

        public override bool Equals(object? obj)
        {
            return obj is Split other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Date, this.Coefficient);
        }
    }
}
