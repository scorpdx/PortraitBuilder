using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace PortraitBuilder.Model.Portrait
{
    public class PortraitData
    {

        private static readonly ILogger logger = LoggingHelper.CreateLogger<PortraitData>();

        /// <summary>
        /// Dictionary of included sprites
        /// Key is the name of the sprite. E.g. GFX_character_background
        /// </summary>
        public Dictionary<string, SpriteDef> Sprites { get; set; } = new Dictionary<string, SpriteDef>();

        /// <summary>
        /// Dictionary of included Portrait Types.
        /// Key is the name of the Portrait Type. E.g. PORTRAIT_westerngfx_male
        /// </summary>
        public Dictionary<string, PortraitType> PortraitTypes { get; set; } = new Dictionary<string, PortraitType>();

        /// <summary>
        /// Dictionary of optional external offsets 
        /// Key is the name of the sprite. E.g. GFX_byzantine_male_mouth
        /// 
        /// Note: external offsets (if any) are applied globally during the merging, and not per content.
        /// </summary>
        public Dictionary<string, Point> Offsets { get; set; } = new Dictionary<string, Point>();

        // Last wins implementation
        public void MergeWith(PortraitData other)
        {
            Sprites = Sprites.Concat(other.Sprites).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.Last().Value);
            PortraitTypes = PortraitTypes.Concat(other.PortraitTypes).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.Last().Value);
            Offsets = Offsets.Concat(other.Offsets).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.Last().Value);
        }

        public int GetFrameCount(PortraitType portraitType, Characteristic characteristic)
        {
            int nbTiles = 0;
            if (characteristic == DefaultCharacteristics.EYE_COLOR)
            {
                nbTiles = portraitType.EyeColours.Count;
            }
            else if (characteristic == DefaultCharacteristics.HAIR_COLOR)
            {
                nbTiles = portraitType.HairColours.Count;
            }
            else
            {
                foreach (Layer layer in portraitType.Layers.Where(layer => layer.Characteristic == characteristic))
                {
                    if (Sprites.TryGetValue(layer.Name, out SpriteDef def))
                    {
                        nbTiles = def.FrameCount;
                        break;
                    }
                    else
                    {
                        logger.LogError("Sprite not found for layer " + layer);
                    }
                }
            }
            return nbTiles;
        }
    }
}
