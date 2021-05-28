namespace Sideways.Tests
{
    using System.Text.Json;
    using NUnit.Framework;

    public static class SettingsTests
    {
        [Test]
        public static void JsonRoundtrip()
        {
            var settings = new Settings
            {
                AlphaVantage = new AlphaVantageSettings
                {
                    ClientSettings = new AlphaVantageClientSettings
                    {
                        ApiKey = "apiKey",
                        MaxCallsPerMinute = 5,
                    },
                },
            };

            var json = JsonSerializer.Serialize(settings);
            var read = JsonSerializer.Deserialize<Settings>(json);
            Assert.AreEqual(settings.AlphaVantage.ClientSettings.ApiKey, read!.AlphaVantage!.ClientSettings.ApiKey);
        }
    }
}
