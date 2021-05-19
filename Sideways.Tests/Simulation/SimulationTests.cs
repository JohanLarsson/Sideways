namespace Sideways.Tests.Simulation
{
    using System;
    using System.Linq;
    using System.Text.Json;
    using NUnit.Framework;

    using Simulation = Sideways.Simulation;

    public static class SimulationTests
    {
        [Test]
        public static void BuyThenSellAll()
        {
            var simulation = Simulation.Create();
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
            var simulation = Simulation.Create();
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

        [Test]
        public static void BuyThenSellTwice()
        {
            var simulation = Simulation.Create();
            simulation.Buy("TSLA", 580f, 30, DateTimeOffset.Now);
            CollectionAssert.AreEqual(new[] { 30 }, simulation.Positions.SelectMany(x => x.Buys).Select(x => x.Shares));
            CollectionAssert.IsEmpty(simulation.Trades);

            simulation.Sell("TSLA", 620f, 10, DateTimeOffset.Now);
            CollectionAssert.AreEqual(new[] { 20 }, simulation.Positions.SelectMany(x => x.Buys).Select(x => x.Shares));
            CollectionAssert.AreEqual(new[] { 10 }, simulation.Trades.SelectMany(x => x.Buys).Select(x => x.Shares));

            simulation.Sell("TSLA", 620f, 20, DateTimeOffset.Now);
            CollectionAssert.IsEmpty(simulation.Positions);
            CollectionAssert.AreEqual(new[] { 10, 20 }, simulation.Trades.SelectMany(x => x.Buys).Select(x => x.Shares));
        }

        [Test]
        public static void BuyTwiceThenSellTwice()
        {
            var simulation = Simulation.Create();
            simulation.Buy("TSLA", 580f, 10, DateTimeOffset.Now);
            CollectionAssert.AreEqual(new[] { 10 }, simulation.Positions.SelectMany(x => x.Buys).Select(x => x.Shares));
            CollectionAssert.IsEmpty(simulation.Trades);

            simulation.Buy("TSLA", 580f, 20, DateTimeOffset.Now);
            CollectionAssert.AreEqual(new[] { 10, 20 }, simulation.Positions.SelectMany(x => x.Buys).Select(x => x.Shares));
            CollectionAssert.IsEmpty(simulation.Trades);

            simulation.Sell("TSLA", 620f, 14, DateTimeOffset.Now);
            CollectionAssert.AreEqual(new[] { 10, 6 }, simulation.Positions.SelectMany(x => x.Buys).Select(x => x.Shares));
            CollectionAssert.AreEqual(new[] { 14 }, simulation.Trades.SelectMany(x => x.Buys).Select(x => x.Shares));

            simulation.Sell("TSLA", 620f, 16, DateTimeOffset.Now);
            CollectionAssert.IsEmpty(simulation.Positions);
            CollectionAssert.AreEqual(new[] { 14, 10, 6 }, simulation.Trades.SelectMany(x => x.Buys).Select(x => x.Shares));
        }

        [Test]
        public static void RoundtripJson()
        {
            var simulation = Simulation.Create();
            simulation.Buy("TSLA", 580f, 10, DateTimeOffset.Now);
            simulation.Buy("TSLA", 580f, 20, DateTimeOffset.Now);
            simulation.Sell("TSLA", 620f, 14, DateTimeOffset.Now);

            var json = JsonSerializer.Serialize(simulation);
            var roundtripped = JsonSerializer.Deserialize<Simulation>(json);

            CollectionAssert.AreEqual(simulation.Positions.SelectMany(x => x.Buys).Select(x => x.Shares), roundtripped!.Positions.SelectMany(x => x.Buys).Select(x => x.Shares));
            CollectionAssert.AreEqual(simulation.Trades.SelectMany(x => x.Buys).Select(x => x.Shares), roundtripped.Trades.SelectMany(x => x.Buys).Select(x => x.Shares));
        }
    }
}
