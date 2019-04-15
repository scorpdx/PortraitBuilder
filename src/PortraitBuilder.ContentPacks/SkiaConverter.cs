using Newtonsoft.Json;
using SkiaSharp;
using System;

namespace PortraitBuilder.ContentPacks
{
    public class SkiaConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is SKColor color)
            {
                serializer.Serialize(writer, color.ToString(), typeof(SKColor));
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (SKColor.TryParse((string)reader.Value, out SKColor color))
            {
                return color;
            }
            return default;
        }

        public override bool CanRead => true;

        public override bool CanConvert(Type objectType) => objectType == typeof(SKColor);
    }
}
