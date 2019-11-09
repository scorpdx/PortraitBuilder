using PortraitBuilder.ContentPacks.Converters;
using System;
using System.Text.Json;

namespace PortraitBuilder.ContentPacks
{
    public static class JsonHelper
    {
        public static JsonSerializerOptions GetDefaultOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new SKColorConverter());
            options.Converters.Add(new SKPointIConverter());
            options.Converters.Add(new HairConverter());
            return options;
        }

        public static void EnsureRead(this ref Utf8JsonReader reader)
        {
            if (!reader.Read()) throw new JsonException("expected token to parse");
        }

        public static void ThrowIncompleteObject(string missingPropertyName)
            => throw new JsonException($"Required member \"{missingPropertyName}\" was not found", new ArgumentNullException(missingPropertyName));

    }
}
