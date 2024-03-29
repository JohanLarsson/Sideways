﻿namespace Sideways
{
    using System;

    [System.Diagnostics.DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public readonly struct FloatRange : IEquatable<FloatRange>
    {
        public readonly float Min;
        public readonly float Max;

        public FloatRange(float min, float max)
        {
            this.Min = min;
            this.Max = max;
        }

        public static bool operator ==(FloatRange left, FloatRange right) => left.Equals(right);

        public static bool operator !=(FloatRange left, FloatRange right) => !left.Equals(right);

        public bool Equals(FloatRange other) => this.Min.Equals(other.Min) && this.Max.Equals(other.Max);

        public override bool Equals(object? obj) => obj is FloatRange other && this.Equals(other);

        public override int GetHashCode() => HashCode.Combine(this.Min, this.Max);

        public bool Contains(float value) => value >= this.Min && value <= this.Max;

        public double Interpolate(float value)
        {
            var range = this.Max - this.Min;
            if (range == 0)
            {
                return 0;
            }

            return (value - this.Min) / range;
        }

        private string GetDebuggerDisplay() => $"Min: {this.Min} Max: {this.Max}";
    }
}
