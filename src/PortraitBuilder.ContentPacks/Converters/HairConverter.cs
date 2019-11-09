using PortraitBuilder.Model.Portrait;
using SkiaSharp;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using static PortraitBuilder.ContentPacks.JsonHelper;

namespace PortraitBuilder.ContentPacks.Converters
{
    public class HairConverter : JsonConverter<Hair>
    {
        public override Hair Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var colorConverter = (JsonConverter<SKColor>)options.GetConverter(typeof(SKColor));
            
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

            SKColor? dark = null, @base = null, highlight = null;

            var startingDepth = reader.CurrentDepth;
            do
            {
                if (!reader.Read())
                {
                    throw new JsonException("expected token to parse");
                }

                switch (reader.TokenType)
                {
                    case JsonTokenType.PropertyName when reader.ValueTextEquals("dark"):
                        dark = colorConverter.Read(ref reader, typeof(SKColor), options);
                        break;
                    case JsonTokenType.PropertyName when reader.ValueTextEquals("base"):
                        @base = colorConverter.Read(ref reader, typeof(SKColor), options);
                        break;
                    case JsonTokenType.PropertyName when reader.ValueTextEquals("highlight"):
                        highlight = colorConverter.Read(ref reader, typeof(SKColor), options);
                        break;
                }
            }
            while (reader.CurrentDepth > startingDepth);

            if (!dark.HasValue)
                ThrowIncompleteObject(nameof(dark));
            if (!@base.HasValue)
                ThrowIncompleteObject(nameof(@base));
            if (!highlight.HasValue)
                ThrowIncompleteObject(nameof(highlight));

            return new Hair(dark.Value, @base.Value, highlight.Value);
        }

        public override void Write(Utf8JsonWriter writer, Hair value, JsonSerializerOptions options)
        {
            var colorConverter = (JsonConverter<SKColor>)options.GetConverter(typeof(SKColor));

            writer.WriteStartObject();

            writer.WritePropertyName("dark");
            colorConverter.Write(writer, value.Dark, options);

            writer.WritePropertyName("base");
            colorConverter.Write(writer, value.Base, options);

            writer.WritePropertyName("highlight");
            colorConverter.Write(writer, value.Highlight, options);

            writer.WriteEndObject();
        }
    }
}
