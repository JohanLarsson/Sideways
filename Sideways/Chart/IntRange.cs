namespace Sideways
{
    using System;

    [System.Diagnostics.DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct IntRange : IEquatable<IntRange>
    {
        public readonly int Min;
        public readonly int Max;

        public IntRange(int min, int max)
        {
            this.Min = min;
            this.Max = max;
        }

        public static bool operator ==(IntRange left, IntRange right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IntRange left, IntRange right)
        {
            return !left.Equals(right);
        }

        public bool Equals(IntRange other)
        {
            return this.Min.Equals(other.Min) && this.Max.Equals(other.Max);
        }

        public override bool Equals(object? obj)
        {
            return obj is IntRange other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Min, this.Max);
        }

        public bool Contains(int value) => value >= this.Min && value <= this.Max;

        private string GetDebuggerDisplay() => $"Min: {this.Min} Max: {this.Max}";
    }
}
