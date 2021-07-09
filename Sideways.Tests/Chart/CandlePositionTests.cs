namespace Sideways.Tests.Chart
{
    using NUnit.Framework;

    public static class CandlePositionTests
    {
        [TestCase(0, 5, 2.5)]
        [TestCase(1, 5, 2.5)]
        [TestCase(1.2, 5, 2.5)]
        [TestCase(2, 5, 2.5)]
        [TestCase(3, 5, 2.5)]
        [TestCase(4, 5, 2.5)]
        [TestCase(4.2, 5, 2.5)]
        [TestCase(5, 5, 7.5)]
        [TestCase(7, 5, 7.5)]
        [TestCase(9, 5, 7.5)]
        public static void SnapCenterX(double x, int candleWidth,  double expected)
        {
            Assert.AreEqual(expected, CandlePosition.SnapCenterX(x, candleWidth));
        }
    }
}
