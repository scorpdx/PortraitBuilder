using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace PortraitBuilder.Model.Portrait
{
    public static class PortraitTypeExtensions
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<PortraitType>();

        /// <summary>
        /// Returns a new PortraitTytpe, with the clothing layers of basePortraitType overriden by clothingPortraitType
        /// </summary>
        public static PortraitType Merge(this PortraitType basePortraitType, PortraitType clothingPortraitType)
        {
            var mergedPortraitType = new PortraitType
            {
                EffectFile = basePortraitType.EffectFile,
                HairColourIndex = basePortraitType.HairColourIndex,
                HairColours = basePortraitType.HairColours,
                HeadgearThatHidesHair = basePortraitType.HeadgearThatHidesHair,
                EyeColours = basePortraitType.EyeColours,
                EyeColourIndex = basePortraitType.EyeColourIndex,

                Name = basePortraitType.Name + "/" + clothingPortraitType.Name,

                Layers = new List<Layer>(basePortraitType.Layers.Count)
            };

            foreach (Layer baseLayer in basePortraitType.Layers)
            {
                var mergeLayer = baseLayer;
                if (baseLayer.CultureIndex != -1)
                {
                    var overrideLayer = clothingPortraitType.GetCultureLayer(baseLayer.CultureIndex);
                    if (overrideLayer != null)
                    {
                        logger.LogDebug("Overriding layer {0} with {1}", baseLayer, overrideLayer);
                        mergeLayer = new Layer
                        {
                            Characteristic = baseLayer.Characteristic,
                            CultureIndex = baseLayer.CultureIndex,
                            IsEye = baseLayer.IsEye,
                            IsHair = baseLayer.IsHair,
                            Name = overrideLayer.Name,
                            Offset = baseLayer.Offset
                        };
                    }
                }
                mergedPortraitType.Layers.Add(mergeLayer);
            }

            return mergedPortraitType;
        }
    }
}
