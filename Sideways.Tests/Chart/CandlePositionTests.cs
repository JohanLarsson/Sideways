namespace Sideways.Tests.Chart
{
    using NUnit.Framework;

    public static class CandlePositionTests
    {
        [TestCase(1, 10, 5, 2.5)]
        [TestCase(1.2, 10, 5, 2.5)]
        [TestCase(2, 10, 5, 2.5)]
        [TestCase(2, 11, 5, 3.5)]
        [TestCase(3, 10, 5, 2.5)]
        [TestCase(4, 10, 5, 2.5)]
        [TestCase(4.2, 10, 5, 2.5)]
        [TestCase(7, 10, 5, 7.5)]
        [TestCase(9, 10, 5, 7.5)]
        public static void SnapCenterX(double x, double actualWidth, int candleWidth,  double expected)
        {
            Assert.AreEqual(expected, CandlePosition.SnapX(x, actualWidth, candleWidth));
        }
    }
}
