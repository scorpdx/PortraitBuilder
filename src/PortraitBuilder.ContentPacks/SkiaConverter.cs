using SkiaSharp;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortraitBuilder.ContentPacks
{
    public class SkiaConverter : JsonConverter<SKColor>
    {
        public override SKColor Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(SKColor.TryParse(reader.GetString(), out SKColor color))
            {
                return color;
            }
            return default;
        }

        public override void Write(Utf8JsonWriter writer, SKColor value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}
