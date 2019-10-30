using System.Collections.Generic;
using PortraitBuilder.Model.Portrait;
using SkiaSharp;
using System.Diagnostics;

namespace PortraitBuilder.Engine
{
    using static ColorHelper;
    /// <summary>
    /// Handles the rendering of portraits
    /// </summary>
    public static class PortraitRenderer
    {
        /// <summary>
        /// Draws a character portrait.
        /// </summary>
        public static SKBitmap DrawPortrait(IEnumerable<PortraitBuilder.TileRenderStep> steps)
        {
            var portraitInfo = new SKImageInfo(176, 176);
            var portraitImage = new SKBitmap(portraitInfo);
            using (var canvas = new SKCanvas(portraitImage))
            {
                //must set transparent bg for unpremul -> premul
                //use white transparent
                canvas.Clear(SKColors.Transparent);
                foreach (var step in steps)
                {
                    DrawTile(canvas, step);
                }
            }

            return portraitImage;
        }

        public static SKBitmap DrawCosmeticStep(PortraitBuilder.TileRenderStep step)
            => step switch
            {
                PortraitBuilder.HairRenderStep hairStep => ShadeHair(step.Tile, hairStep.Hair),
                PortraitBuilder.EyeRenderStep eyeStep => ShadeEye(step.Tile, eyeStep.EyeColor),
                _ => step.Tile,
            };

        public static void DrawTile(SKCanvas canvas, PortraitBuilder.TileRenderStep step)
        {
            //FIXME: this causes AVE?
            /*using */
            var tile = DrawCosmeticStep(step);
            canvas.DrawBitmap(tile, step.TileOffset);
        }

        /// <summary>
        /// Based on gfx\FX\portrait.lua EyePixelShader
        /// </summary>
        public static SKBitmap ShadeEye(SKBitmap source, SKColor eyeColor)
        {
            var output = source.Copy();

            unsafe
            {
                SKColor* oColor = (SKColor*)output.GetPixels().ToPointer();

                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++, oColor++)
                    {
                        if (oColor->Alpha == 0) continue;

                        var final = new SKColor(blue: (byte)(255 * ((eyeColor.Blue / 255d) * (oColor->Red / 255d))),
                                                green: (byte)(255 * ((eyeColor.Green / 255d) * (oColor->Red / 255d))),
                                                red: (byte)(255 * ((eyeColor.Red / 255d) * (oColor->Red / 255d))),
                                                alpha: oColor->Alpha);
                        *oColor = final;
                    }
                }
            }

            return output;
        }

        /// <summary>
        /// Based on gfx\FX\portrait.lua HairPixelShader
        /// </summary>
        public static SKBitmap ShadeHair(SKBitmap source, Hair hair)
        {
            var output = source.Copy();

            unsafe
            {
                SKColor* oColor = (SKColor*)output.GetPixels().ToPointer();

                for (int y = 0; y < source.Height; y++)
                {
                    for (int x = 0; x < source.Width; x++, oColor++)
                    {
                        if (oColor->Alpha == 0) continue;

                        var lerp1 = Lerp(hair.Dark, hair.Base, Clamp(oColor->Green * 2d));
                        var lerp2 = Lerp(lerp1, hair.Highlight, Clamp((oColor->Green - 128d) * 2));
                        var final = lerp2.WithAlpha(oColor->Alpha);

                        *oColor = final;
                    }
                }
            }

            return output;
        }
    }
}
