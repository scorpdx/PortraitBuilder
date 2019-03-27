using System;
using System.Collections.Generic;
using System.IO;
using PortraitBuilder.Model.Portrait;
using PortraitBuilder.Model.Content;
using SkiaSharp;
using System.Diagnostics;
using PortraitBuilder.Model;
using Microsoft.Extensions.Logging;

namespace PortraitBuilder.Engine
{
    using static ColorHelper;
    /// <summary>
    /// Handles the rendering of portraits
    /// </summary>
    public class PortraitRenderer
    {

        private static readonly ILogger logger = LoggingHelper.CreateLogger<PortraitRenderer>();

        private static IReadOnlyDictionary<GovernmentType, string> GovernmentSpriteSuffix { get; } = new Dictionary<GovernmentType, string>
        {
            { GovernmentType.Feudal, "" },
            { GovernmentType.Iqta, "_iqta"},
            { GovernmentType.Theocracy, "_theocracy"},
            { GovernmentType.Republic, "_republic"},
            { GovernmentType.MerchantRepublic, "_merchantrepublic"},
            { GovernmentType.Tribal, "_tribal"},
            { GovernmentType.Nomadic, "_nomadic"},
            { GovernmentType.MonasticFeudal, "_theocraticfeudal"},
            { GovernmentType.ChineseImperial, "_chineseimperial" },
            { GovernmentType.ConfucianBureaucracy, "_confucian" },
        };

        /// <summary>
        /// Draws a character portrait.
        /// </summary>
        /// <param name="portraitType">PortaitType to use for drawing.</param>
        /// <param name="character">Portrait input to draw.</param>
        /// <param name="activeContents">Content to load sprites from</param>
        /// <returns>Frameless portrait drawn with the given parameters.</returns>
        public SKBitmap DrawCharacter(Character character, List<Content> activeContents, Dictionary<string, Sprite> sprites)
        {
            logger.LogInformation($"Drawing Portrait {character}");

            var portraitInfo = new SKImageInfo(176, 176);
            var portraitImage = new SKBitmap(portraitInfo);
            using (var canvas = new SKCanvas(portraitImage))
            {
                //must set transparent bg for unpremul -> premul
                canvas.Clear(SKColors.Transparent);

                foreach (var layer in character.PortraitType.Layers)
                {
                    if (!DrawLayer(layer, canvas, character, activeContents, sprites))
                    {
                        logger.LogWarning($"Could not render layer {layer}");
                    }
                }

                DrawBorder(character, canvas, activeContents, sprites);
            }
            return portraitImage;
        }

        private bool DrawLayer(Layer layer, SKCanvas canvas, Character character, List<Content> activeContents, Dictionary<string, Sprite> sprites)
        {
            logger.LogDebug($"Drawing Layer : {layer}");

            // Backup for merchants, which are part of "The Republic" DLC !
            string spriteName = GetOverriddenSpriteName(character, layer);
            if (!sprites.TryGetValue(spriteName, out Sprite sprite) && !sprites.TryGetValue(layer.Name, out sprite))
            {
                logger.LogError("Sprite not found. spriteName: {0}, layerName: {1}", spriteName, layer.Name);
                return false;
            }

            //Check if loaded; if not, then load
            if (!sprite.IsLoaded)
                LoadSprite(sprite, activeContents);

            //Get DNA/Properties letter, then the index of the tile to draw
            return TryGetTileIndex(character, sprite.FrameCount, layer, out int tileIndex)
                && DrawTile(character, canvas, sprite, layer, tileIndex);
        }

        /// <summary>
        /// Override sprite for religious/merchant
        /// 
        /// This is quite messy - not sure what hardcoded vanilla logic exactly is - could be based on culture index ?!
        /// </summary>
        /// <param name="character"></param>
        /// <param name="layer"></param>
        /// <returns></returns>
        private string GetOverriddenSpriteName(Character character, Layer layer)
        {
            string spriteName = layer.Name;

            var hasSpecialGovernment = character.Government == GovernmentType.Theocracy || character.Government == GovernmentType.MerchantRepublic;
            var isOutfitLayer = layer.Characteristic == Characteristic.CLOTHES || layer.Characteristic == Characteristic.HEADGEAR;

            if (hasSpecialGovernment && isOutfitLayer)
            {
                string sex = character.Sex == Sex.Male ? "male" : "female";
                string layerSuffix = spriteName.Contains("behind") ? "_behind" : ""; // Handles clothes_infront and headgear_mid
                string government = character.Government == GovernmentType.Theocracy ? "religious" : "merchant";
                string layerType = layer.Characteristic == Characteristic.CLOTHES ? "clothes" : "headgear";
                spriteName = $"GFX_{government}_{sex}_{layerType}{layerSuffix}";
            }

            return spriteName;
        }

        private void DrawBorder(Character character, SKCanvas canvas, List<Content> activeContents, Dictionary<string, Sprite> sprites)
        {
            logger.LogDebug("Drawing border.");
            try
            {
                string governmentSpriteName = "GFX_charframe_150" + GovernmentSpriteSuffix[character.Government];
                if (sprites.ContainsKey(governmentSpriteName))
                {
                    Sprite sprite = sprites[governmentSpriteName];

                    //Check if loaded; if not, then load
                    if (!sprite.IsLoaded)
                    {
                        LoadSprite(sprite, activeContents);
                    }
                    canvas.DrawBitmap(sprite.Tiles[character.Rank], SKPoint.Empty);
                }
            }
            catch (Exception e)
            {
                logger.LogError("Could not render borders ", e);
            }
        }

        private bool TryGetTileIndex(Character character, int frameCount, Layer layer, out int tileIndex)
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

        private void LoadSprite(Sprite sprite, List<Content> activeContents)
        {
            // Paths in vanilla files are Windows-style
            string filePath = sprite.TextureFilePath.Replace('\\', Path.DirectorySeparatorChar);

            // Also try alternative extension (.tga <=> .dds)
            string extension = filePath.Substring(filePath.Length - 4);
            string alternative = extension == ".dds" ? ".tga" : ".dds";
            string alternativeFilePath = filePath.Replace(extension, alternative);

            string path = null;

            // Loop on reverse order - last occurence wins if asset is overriden !
            for (int i = activeContents.Count - 1; i >= 0; i--)
            {
                Content content = activeContents[i];
                path = content.AbsolutePath + Path.DirectorySeparatorChar + filePath;

                if (!File.Exists(path))
                {
                    path = content.AbsolutePath + Path.DirectorySeparatorChar + alternativeFilePath;
                }
                if (File.Exists(path))
                {
                    break;
                }
            }

            if (path != null)
            {
                logger.LogDebug("Loading sprite from: " + path);
                sprite.Load(path);
            }
            else
            {
                throw new FileNotFoundException(string.Format("Unable to find file: {0} under active content {1}", filePath, activeContents));
            }
        }

        private bool DrawTile(Character character, SKCanvas canvas, Sprite sprite, Layer layer, int tileIndex)
        {
            SKBitmap tile;
            if (layer.IsHair)
            {
                var hairColors = character.PortraitType.HairColours;
                if (!character.TryGetLetter(Characteristic.HAIR_COLOR, out char hairChar))
                {
                    logger.LogError("Letter not found. character {0} characteristic {1}", character, Characteristic.HAIR_COLOR);
                    return false;
                }
                int hairIndex = Character.GetIndex(hairChar, hairColors.Count);
                tile = DrawHair(sprite.Tiles[tileIndex], hairColors[hairIndex]);
            }
            else if (layer.IsEye)
            {
                var eyeColors = character.PortraitType.EyeColours;
                if (!character.TryGetLetter(Characteristic.EYE_COLOR, out char eyeChar))
                {
                    logger.LogError("Letter not found. character {0} characteristic {1}", character, Characteristic.EYE_COLOR);
                    return false;
                }
                int eyeIndex = Character.GetIndex(eyeChar, eyeColors.Count);
                tile = DrawEye(sprite.Tiles[tileIndex], eyeColors[eyeIndex]);
            }
            else
            {
                tile = sprite.Tiles[tileIndex];
            }

            var p = new SKPointI(12 + layer.Offset.X, 12 + 152 - tile.Height - layer.Offset.Y);
            canvas.DrawBitmap(tile, p);

            return true;
        }

        /// <summary>
        /// Based on gfx\FX\portrait.lua EyePixelShader
        /// </summary>
        private SKBitmap DrawEye(SKBitmap source, SKColor eyeColor)
        {
            var output = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);
            output.Erase(SKColors.Transparent);

            Debug.Assert(source.BytesPerPixel == 4 /* sizeof(BGRA) */);

            unsafe
            {
                SKPMColor* sColor = (SKPMColor*)source.GetPixels().ToPointer();
                SKPMColor* oColor = (SKPMColor*)output.GetPixels().ToPointer();

                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++, sColor++, oColor++)
                    {
                        if (sColor->Alpha == 0) continue;

                        var final = new SKColor(blue: (byte)(255 * ((eyeColor.Blue / 255d) * (sColor->Red / 255d))),
                                                green: (byte)(255 * ((eyeColor.Green / 255d) * (sColor->Red / 255d))),
                                                red: (byte)(255 * ((eyeColor.Red / 255d) * (sColor->Red / 255d))),
                                                alpha: sColor->Alpha);
                        *oColor = SKPMColor.PreMultiply(final);
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Based on gfx\FX\portrait.lua HairPixelShader
        /// </summary>
        private SKBitmap DrawHair(SKBitmap source, Hair hair)
        {
            var output = new SKBitmap(source.Width, source.Height, source.ColorType, source.AlphaType);
            output.Erase(SKColors.Transparent);

            Debug.Assert(source.BytesPerPixel == 4 /* sizeof(BGRA) */);

            unsafe
            {
                SKPMColor* sColor = (SKPMColor*)source.GetPixels().ToPointer();
                SKPMColor* oColor = (SKPMColor*)output.GetPixels().ToPointer();

                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++, sColor++, oColor++)
                    {
                        if (sColor->Alpha == 0) continue;

                        var lerp1 = Lerp(hair.Dark, hair.Base, Clamp(sColor->Green * 2d));
                        var lerp2 = Lerp(lerp1, hair.Highlight, Clamp((sColor->Green - 128d) * 2));
                        var final = lerp2.WithAlpha(sColor->Alpha);

                        *oColor = SKPMColor.PreMultiply(final);
                    }
                }
            }

            return output;
        }
    }
}
