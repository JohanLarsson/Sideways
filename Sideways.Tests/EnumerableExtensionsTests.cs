namespace Sideways.Tests
{
    using NUnit.Framework;

    public static class EnumerableExtensionsTests
    {
        [TestCase(new[] { 1f }, 2, new float[0])]
        [TestCase(new[] { 1f, 2f }, 2, new[] { 1.5f })]
        [TestCase(new[] { 1f, 2f, 3f }, 2, new[] { 1.5f, 2.5f })]
        [TestCase(new[] { 1f, 2f, 3f, 4f }, 2, new[] { 1.5f, 2.5f, 3.5f })]
        [TestCase(new[] { 1f, 2f, 3f, 4f, 5f }, 2, new[] { 1.5f, 2.5f, 3.5f, 4.5f })]
        [TestCase(new[] { 1f }, 3, new float[0])]
        [TestCase(new[] { 1f, 2f }, 3, new float[0])]
        [TestCase(new[] { 1f, 2f, 3f }, 3, new[] { 2f })]
        [TestCase(new[] { 1f, 2f, 3f, 4f }, 3, new[] { 2f, 3f })]
        [TestCase(new[] { 1f, 2f, 3f, 4f, 5f }, 3, new[] { 2f, 3f, 4f })]
        public static void MovingAverage(float[] xs, int period, float[] expected)
        {
            CollectionAssert.AreEqual(expected, xs.MovingAverage(period, x => x));
        }

        [TestCase(new[] { 1.0 }, 2, new double[0])]
        [TestCase(new[] { 1.0, 2.0 }, 2, new[] { 1.5})]
        [TestCase(new[] { 1.0, 2.0, 3.0 }, 2, new[] { 1.5, 2.5 })]
        [TestCase(new[] { 1.0, 2.0, 3.0, 4.0 }, 2, new[] { 1.5, 2.5, 3.5 })]
        [TestCase(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 2, new[] { 1.5, 2.5, 3.5, 4.5 })]
        [TestCase(new[] { 1.0 }, 3, new double[0])]
        [TestCase(new[] { 1.0, 2.0 }, 3, new double[0])]
        [TestCase(new[] { 1.0, 2.0, 3.0 }, 3, new[] { 2.0 })]
        [TestCase(new[] { 1.0, 2.0, 3.0, 4.0 }, 3, new[] { 2.0, 3.0 })]
        [TestCase(new[] { 1.0, 2.0, 3.0, 4.0, 5.0 }, 3, new[] { 2.0, 3.0, 4.0 })]
        public static void MovingAverage(double[] xs, int period, double[] expected)
        {
            CollectionAssert.AreEqual(expected, xs.MovingAverage(period));
        }
    }
}
