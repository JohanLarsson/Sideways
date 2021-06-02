namespace Sideways.AlphaVantage
{
    using System;
    using System.Buffers.Text;
    using System.Text.Json;

    internal static class JsonExtensions
    {
        internal static bool ValueTextStartsWith(this ref Utf8JsonReader reader, string text)
        {
            if (text.Length < reader.ValueSpan.Length)
            {
                for (var i = 0; i < text.Length; i++)
                {
                    if (reader.ValueSpan[i] != text[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        internal static void Read(this ref Utf8JsonReader reader, JsonTokenType tokenType)
        {
            if (reader.TokenType == tokenType)
            {
                reader.Read();
                return;
            }

            throw new JsonException($"Expected {tokenType}");
        }

        internal static DateTimeOffset ReadPropertyNameAsDate(this ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                if (reader.ValueSpan.Length == 10 &&
                    Utf8Parser.TryParse(reader.ValueSpan[..4], out int year, out _) &&
                    Utf8Parser.TryParse(reader.ValueSpan.Slice(5, 2), out int month, out _) &&
                    Utf8Parser.TryParse(reader.ValueSpan.Slice(8, 2), out int day, out _))
                {
                    reader.Read(JsonTokenType.PropertyName);
                    reader.Read(JsonTokenType.StartObject);
                    return new DateTimeOffset(year, month, day, 0, 0, 0, 0, TimeSpan.Zero);
                }

                if (reader.ValueSpan.Length == 19 &&
                    Utf8Parser.TryParse(reader.ValueSpan[..4], out year, out _) &&
                    Utf8Parser.TryParse(reader.ValueSpan.Slice(5, 2), out month, out _) &&
                    Utf8Parser.TryParse(reader.ValueSpan.Slice(8, 2), out day, out _) &&
                    Utf8Parser.TryParse(reader.ValueSpan.Slice(11, 2), out int hour, out _) &&
                    Utf8Parser.TryParse(reader.ValueSpan.Slice(14, 2), out int minute, out _) &&
                    Utf8Parser.TryParse(reader.ValueSpan.Slice(17, 2), out int second, out _))
                {
                    reader.Read(JsonTokenType.PropertyName);
                    reader.Read(JsonTokenType.StartObject);
                    return new DateTimeOffset(year, month, day, hour, minute, second, 0, TimeSpan.Zero);
                }

                throw new JsonException($"Error parsing date");
            }

            throw new JsonException($"Expected {JsonTokenType.PropertyName}");
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

        internal static int ReadInt32(this ref Utf8JsonReader reader, string propertyName)
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
                Utf8Parser.TryParse(reader.ValueSpan, out int result, out _))
            {
                reader.Read(JsonTokenType.String);
                return result;
            }

            throw new JsonException($"Error parsing int");
        }
    }
}
