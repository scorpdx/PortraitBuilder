using Microsoft.Extensions.Logging;
using PortraitBuilder.Model;
using PortraitBuilder.Model.Portrait;
using SkiaSharp;
using System.Collections.Generic;

namespace PortraitBuilder.Engine
{
    public static class PortraitBuilder
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger(nameof(PortraitBuilder));

        private const string GovernmentSpritePrefix = "GFX_charframe_150";
        private static IReadOnlyDictionary<GovernmentType, string> GovernmentSpriteNames { get; } = new Dictionary<GovernmentType, string>
        {
            { GovernmentType.Feudal, GovernmentSpritePrefix },
            { GovernmentType.Iqta, $"{GovernmentSpritePrefix}_iqta"},
            { GovernmentType.Theocracy, $"{GovernmentSpritePrefix}_theocracy"},
            { GovernmentType.Republic, $"{GovernmentSpritePrefix}_republic"},
            { GovernmentType.MerchantRepublic, $"{GovernmentSpritePrefix}_merchantrepublic"},
            { GovernmentType.Tribal, $"{GovernmentSpritePrefix}_tribal"},
            { GovernmentType.Nomadic, $"{GovernmentSpritePrefix}_nomadic"},
            { GovernmentType.MonasticFeudal, $"{GovernmentSpritePrefix}_theocraticfeudal"},
            { GovernmentType.ChineseImperial, $"{GovernmentSpritePrefix}_chineseimperial" },
            { GovernmentType.ConfucianBureaucracy, $"{GovernmentSpritePrefix}_confucian" },
        };

        public class TileRenderStep
        {
            public SpriteDef Def { get; set; }
            public int TileIndex { get; set; }
            public SKBitmap? Tile { get; set; }

            private SKPointI? tileOffset;
            //var p = new SKPointI(12 + layer.Offset.X, 12 + 152 - tile.Height - layer.Offset.Y);
            public SKPointI TileOffset
            {
                get
                {
                    if (!tileOffset.HasValue)
                        return SKPointI.Empty;

                    return Tile != null ? SKPointI.Subtract(tileOffset.Value, new SKPointI(0, Tile.Height)) : tileOffset.Value;
                }
                set => tileOffset = value;
            }
        }

        public class HairRenderStep : TileRenderStep
        {
            public Hair Hair;
        }

        public class EyeRenderStep : TileRenderStep
        {
            public SKColor EyeColor;
        }

        public static IEnumerable<TileRenderStep> BuildCharacter(Character character, Dictionary<string, SpriteDef> sprites)
        {
            foreach (var layer in character.PortraitType.Layers)
            {
                yield return BuildLayer(layer, character, sprites);
            }

            yield return BuildBorder(character, sprites);
        }

        private static TileRenderStep? BuildLayer(Layer layer, Character character, Dictionary<string, SpriteDef> sprites)
        {
            // Backup for merchants, which are part of "The Republic" DLC !
            string spriteName = GetOverriddenSpriteName(character, layer);
            if (!sprites.TryGetValue(spriteName, out SpriteDef def) && !sprites.TryGetValue(layer.Name, out def))
                return null;

            //Get DNA/Properties letter, then the index of the tile to draw
            if (!TryGetTileIndex(character, def.FrameCount, layer, out int tileIndex))
                return null;

            return BuildTile(character, layer, def, tileIndex);
        }

        private static string GetOverriddenSpriteName(Character character, Layer layer)
        {
            string spriteName = layer.Name;

            var hasSpecialGovernment = character.Government == GovernmentType.Theocracy || character.Government == GovernmentType.MerchantRepublic;
            var isOutfitLayer = layer.Characteristic == DefaultCharacteristics.CLOTHES || layer.Characteristic == DefaultCharacteristics.HEADGEAR;

            if (hasSpecialGovernment && isOutfitLayer)
            {
                string sex = character.Sex == Sex.Male ? "male" : "female";
                string layerSuffix = spriteName.Contains("behind") ? "_behind" : ""; // Handles clothes_infront and headgear_mid
                string government = character.Government == GovernmentType.Theocracy ? "religious" : "merchant";
                string layerType = layer.Characteristic == DefaultCharacteristics.CLOTHES ? "clothes" : "headgear";
                spriteName = $"GFX_{government}_{sex}_{layerType}{layerSuffix}";
            }

            return spriteName;
        }

        public static TileRenderStep? BuildBorder(Character character, Dictionary<string, SpriteDef> sprites)
        {
            string governmentSpriteName = GovernmentSpriteNames[character.Government];
            if (!sprites.TryGetValue(governmentSpriteName, out SpriteDef def))
                return null;

            return new TileRenderStep
            {
                Def = def,
                TileIndex = (int)character.Rank,
            };
        }

        private static bool TryGetTileIndex(Character character, int frameCount, Layer layer, out int tileIndex)
        {
            tileIndex = default;
            if (!character.TryGetLetter(layer.Characteristic, out char letter))
            {
                logger.LogWarning("Letter not found. character {0} layer {1} characteristic {2}", character, layer, layer.Characteristic);
                return false;
            }

            tileIndex = Character.GetIndex(letter, frameCount);
            logger.LogDebug($"Layer letter: {letter}, Tile Index: {tileIndex}");

            return true;
        }

        private static TileRenderStep? BuildTile(Character character, Layer layer, SpriteDef def, int tileIndex)
        {
            if (layer.IsHair)
            {
                var hairColors = character.PortraitType.HairColours;
                if (!character.TryGetLetter(DefaultCharacteristics.HAIR_COLOR, out char hairChar))
                {
                    logger.LogError("Letter not found. character {0} characteristic {1}", character, DefaultCharacteristics.HAIR_COLOR);
                    return null;
                }

                int hairIndex = Character.GetIndex(hairChar, hairColors.Count);
                var tileOffset = new SKPointI(12 + layer.Offset.X, 12 + 152 - layer.Offset.Y);
                return new HairRenderStep
                {
                    Def = def,
                    TileIndex = tileIndex,
                    Hair = hairColors[hairIndex],
                    TileOffset = tileOffset,
                };
            }
            else if (layer.IsEye)
            {
                var eyeColors = character.PortraitType.EyeColours;
                if (!character.TryGetLetter(DefaultCharacteristics.EYE_COLOR, out char eyeChar))
                {
                    logger.LogError("Letter not found. character {0} characteristic {1}", character, DefaultCharacteristics.EYE_COLOR);
                    return null;
                }

                int eyeIndex = Character.GetIndex(eyeChar, eyeColors.Count);
                var tileOffset = new SKPointI(12 + layer.Offset.X, 12 + 152 - layer.Offset.Y);
                return new EyeRenderStep
                {
                    Def = def,
                    TileIndex = tileIndex,
                    EyeColor = eyeColors[eyeIndex],
                    TileOffset = tileOffset,
                };
            }
            else
            {
                return BuildSpriteTile(character, def, tileIndex, layer.Offset);
            }
        }

        private static TileRenderStep? BuildSpriteTile(Character character, SpriteDef def, int tileIndex, System.Drawing.Point offset = default)
        {
            var tileOffset = new SKPointI(12 + offset.X, 12 + 152 /*- tile.Height*/ - offset.Y);
            return new TileRenderStep
            {
                Def = def,
                TileIndex = tileIndex,
                TileOffset = tileOffset,
            };
        }
    }
}
