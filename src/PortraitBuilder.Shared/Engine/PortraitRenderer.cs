using System.Collections.Generic;
using PortraitBuilder.Model.Portrait;
using SkiaSharp;
using System.Diagnostics;
using System;

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
            var portraitImage = new SKBitmap(portraitInfo, SKBitmapAllocFlags.ZeroPixels);

            using var canvas = new SKCanvas(portraitImage);
            foreach (var step in steps)
            {
                Debug.Assert(step.Tile != null);
                DrawTile(canvas, step);
            }
            canvas.Flush();

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
        public static unsafe SKBitmap ShadeEye(SKBitmap source, SKColor eyeColor)
        {
            var output = source.Copy();

            var pixelAddr = output.GetPixels();
            Debug.Assert(pixelAddr != IntPtr.Zero);

            SKColor* oColor = (SKColor*)pixelAddr.ToPointer();
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

            return output;
        }

        /// <summary>
        /// Based on gfx\FX\portrait.lua HairPixelShader
        /// </summary>
        public static unsafe SKBitmap ShadeHair(SKBitmap source, Hair hair)
        {
            var output = source.Copy();

            var pixelAddr = output.GetPixels();
            Debug.Assert(pixelAddr != IntPtr.Zero);

            var oColor = (SKColor*)pixelAddr.ToPointer();
            for (int y = 0; y < output.Height; y++)
            {
                for (int x = 0; x < output.Width; x++, oColor++)
                {
                    if (oColor->Alpha == 0) continue;

                    var lerp1 = Lerp(hair.Dark, hair.Base, Clamp(oColor->Green * 2d));
                    var lerp2 = Lerp(lerp1, hair.Highlight, Clamp((oColor->Green - 128d) * 2));
                    var final = lerp2.WithAlpha(oColor->Alpha);

                    *oColor = final;
                }
            }

            return output;
        }
    }
}
