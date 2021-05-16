namespace Sideways.Tests.Simulation
{
    using System;
    using System.Linq;

    using NUnit.Framework;

    using Simulation = Sideways.Simulation;

    public static class SimulationTests
    {
        [Test]
        public static void BuyThenSellAll()
        {
            var simulation = new Simulation();
            simulation.Buy("TSLA", 580f, 10, DateTimeOffset.Now);
            CollectionAssert.AreEqual(new[] { 10 }, simulation.Positions.SelectMany(x => x.Buys).Select(x => x.Shares));
            CollectionAssert.IsEmpty(simulation.Trades);

            simulation.Sell("TSLA", 620f, 10, DateTimeOffset.Now);
            CollectionAssert.IsEmpty(simulation.Positions);
            CollectionAssert.AreEqual(new[] { 10 }, simulation.Trades.SelectMany(x => x.Buys).Select(x => x.Shares));
        }

        [Test]
        public static void BuyTwiceThenSellAll()
        {
            var simulation = new Simulation();
            simulation.Buy("TSLA", 580f, 10, DateTimeOffset.Now);
            CollectionAssert.AreEqual(new[] { 10 }, simulation.Positions.SelectMany(x => x.Buys).Select(x => x.Shares));
            CollectionAssert.IsEmpty(simulation.Trades);

            simulation.Buy("TSLA", 580f, 20, DateTimeOffset.Now);
            CollectionAssert.AreEqual(new[] { 10, 20 }, simulation.Positions.SelectMany(x => x.Buys).Select(x => x.Shares));
            CollectionAssert.IsEmpty(simulation.Trades);

            simulation.Sell("TSLA", 620f, 30, DateTimeOffset.Now);
            CollectionAssert.IsEmpty(simulation.Positions);
            CollectionAssert.AreEqual(new[] { 10, 20 }, simulation.Trades.SelectMany(x => x.Buys).Select(x => x.Shares));
        }
    }
}
