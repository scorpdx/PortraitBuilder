using SkiaSharp;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortraitBuilder.ContentPacks.Converters
{
    public class SKColorConverter : JsonConverter<SKColor>
    {
        public override SKColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (SKColor.TryParse(reader.GetString(), out SKColor color))
            {
                return color;
            }

            throw new JsonException("Could not parse hex color");
        }

        public override void Write(Utf8JsonWriter writer, SKColor value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
