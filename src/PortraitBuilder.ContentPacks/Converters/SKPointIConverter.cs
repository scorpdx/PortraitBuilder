using SkiaSharp;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using static PortraitBuilder.ContentPacks.JsonHelper;

namespace PortraitBuilder.ContentPacks.Converters
{
    public class SKPointIConverter : JsonConverter<SKPointI>
    {
        public override SKPointI Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                case JsonTokenType.PropertyName:
                    if (!reader.Read())
                    {
                        throw new JsonException("expected token to parse");
                    }
                    break;
            }

            int? x = null, y = null;

            var startingDepth = reader.CurrentDepth;
            do
            {
                if (!reader.Read())
                {
                    throw new JsonException("expected token to parse");
                }

                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName when reader.ValueTextEquals("X"):
                        reader.EnsureRead();
                        x = reader.GetInt32();
                        break;
                    case JsonTokenType.PropertyName when reader.ValueTextEquals("Y"):
                        reader.EnsureRead();
                        y = reader.GetInt32();
                        break;
                }
            }
            while (reader.CurrentDepth > startingDepth);

            if (!x.HasValue)
                ThrowIncompleteObject(nameof(x));
            if (!y.HasValue)
                ThrowIncompleteObject(nameof(y));

            return new SKPointI(x.Value, y.Value);
        }

        public override void Write(Utf8JsonWriter writer, SKPointI value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteNumberValue(value.X);
            writer.WritePropertyName("Y");
            writer.WriteNumberValue(value.Y);
            writer.WriteEndObject();
        }
    }
}
