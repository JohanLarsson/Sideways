namespace Sideways.AlphaVantage
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal sealed class QuarterlyEarningConverter : JsonConverter<QuarterlyEarning>
    {
        internal static readonly QuarterlyEarningConverter Default = new();

        public override QuarterlyEarning Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read(JsonTokenType.StartObject);
            var quarterlyEarning = new QuarterlyEarning(
                reader.ReadDate("fiscalDateEnding"),
                reader.ReadDate("reportedDate"),
                reader.ReadFloat("reportedEPS"),
                reader.ReadFloat("estimatedEPS"));
            _ = reader.ReadFloat("surprise");
            _ = reader.ReadFloat("surprisePercentage");
            return quarterlyEarning;
        }

        public override void Write(Utf8JsonWriter writer, QuarterlyEarning value, JsonSerializerOptions options) => throw new NotSupportedException("No need for this");
    }
}
