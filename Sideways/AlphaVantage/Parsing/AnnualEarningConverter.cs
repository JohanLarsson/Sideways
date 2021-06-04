namespace Sideways.AlphaVantage
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    internal sealed class AnnualEarningConverter : JsonConverter<AnnualEarning>
    {
        internal static readonly AnnualEarningConverter Default = new();

        public override AnnualEarning Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read(JsonTokenType.StartObject);
            return new(
                reader.ReadDate("fiscalDateEnding"),
                reader.ReadFloat("reportedEPS"));
        }

        public override void Write(Utf8JsonWriter writer, AnnualEarning value, JsonSerializerOptions options) => throw new NotSupportedException("No need for this");
    }
}
