namespace Sideways.AlphaVantage
{
    using System;
    using System.Buffers.Text;
    using System.Text.Json;

    internal static class JsonExtensions
    {
        internal static void Read(this ref Utf8JsonReader reader, JsonTokenType tokenType)
        {
            if (reader.TokenType == tokenType)
            {
                reader.Read();
                return;
            }

            throw new JsonException($"Expected {tokenType}");
        }

        internal static DateTimeOffset ReadDate(this ref Utf8JsonReader reader, string propertyName)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected {JsonTokenType.PropertyName}");
            }

            if (!reader.ValueTextEquals(propertyName))
            {
                throw new JsonException($"Expected {propertyName}");
            }

            reader.Read(JsonTokenType.PropertyName);
            if (reader.TokenType == JsonTokenType.String &&
                reader.ValueSpan.Length == 10 &&
                Utf8Parser.TryParse(reader.ValueSpan[..4], out int year, out _) &&
                Utf8Parser.TryParse(reader.ValueSpan.Slice(5, 2), out int month, out _) &&
                Utf8Parser.TryParse(reader.ValueSpan.Slice(8, 2), out int day, out _))
            {
                reader.Read(JsonTokenType.String);
                return new DateTimeOffset(year, month, day, 0, 0, 0, 0, TimeSpan.Zero);
            }

            throw new JsonException($"Error parsing DateTimeOffset");
        }

        internal static float ReadFloat(this ref Utf8JsonReader reader, string propertyName)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected {JsonTokenType.PropertyName}");
            }

            if (!reader.ValueTextEquals(propertyName))
            {
                throw new JsonException($"Expected {propertyName}");
            }

            reader.Read(JsonTokenType.PropertyName);
            if (reader.TokenType == JsonTokenType.String &&
                Utf8Parser.TryParse(reader.ValueSpan, out float result, out _))
            {
                reader.Read(JsonTokenType.String);
                return result;
            }

            throw new JsonException($"Error parsing float");
        }

        internal static float? ReadFloatOrNull(this ref Utf8JsonReader reader, string propertyName)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException($"Expected {JsonTokenType.PropertyName}");
            }

            if (!reader.ValueTextEquals(propertyName))
            {
                throw new JsonException($"Expected {propertyName}");
            }

            reader.Read(JsonTokenType.PropertyName);
            if (reader.TokenType == JsonTokenType.String)
            {
                if (Utf8Parser.TryParse(reader.ValueSpan, out float result, out _))
                {
                    reader.Read(JsonTokenType.String);
                    return result;
                }

                if (reader.ValueTextEquals("None"))
                {
                    reader.Read(JsonTokenType.String);
                    return null;
                }
            }

            throw new JsonException($"Error parsing float");
        }
    }
}
