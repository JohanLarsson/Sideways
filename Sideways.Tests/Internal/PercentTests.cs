namespace Sideways.Tests.Internal
{
    using System.Globalization;

    using NUnit.Framework;

    public static class PercentTests
    {
        [TestCase(1f, 1f, "0.0%")]
        [TestCase(1f, 2f, "100.0%")]
        public static void From(float before, float after, string expected)
        {
            Assert.AreEqual(expected, Percent.From(before, after).ToString("F1", CultureInfo.InvariantCulture));
        }
    }
}
