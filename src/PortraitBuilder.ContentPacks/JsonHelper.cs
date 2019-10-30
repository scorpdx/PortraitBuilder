using PortraitBuilder.ContentPacks.Converters;
using System.Text.Json;

namespace PortraitBuilder.ContentPacks
{
    public static class JsonHelper
    {
        public static JsonSerializerOptions GetDefaultOptions()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new SKColorConverter());
            options.Converters.Add(new HairConverter());
            return options;
        }
    }
}
