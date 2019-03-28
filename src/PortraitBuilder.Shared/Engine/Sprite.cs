using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PortraitBuilder.Engine
{
    public class Sprite : IDisposable
    {
        private static IEnumerable<SKBitmap> LoadTiles(SKBitmap texture, int frameCount)
        {
            var src = new SKRectI(0, 0, texture.Width / frameCount, texture.Height);
            var dst = new SKRectI(0, 0, src.Width, src.Height);

            for (int i = 0; i < frameCount; i++)
            {
                var tile = new SKBitmap(src.Width, src.Height);
                using (var canvas = new SKCanvas(tile))
                {
                    //must set transparent bg for unpremul -> premul
                    canvas.Clear(SKColors.Transparent);
                    canvas.DrawBitmap(texture, src, dst);
                }
                yield return tile;

                src.Offset(src.Width, 0);
            }
        }

        private static SKBitmap LoadFile(string filepath)
        {
            var image = Pfim.Pfim.FromFile(filepath);
            Debug.Assert(image.Format == Pfim.ImageFormat.Rgba32);
            Debug.Assert(image.Compressed == false);

            var info = new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            var bmp = new SKBitmap(info);
            unsafe
            {
                fixed (byte* pData = image.Data)
                {
                    bmp.InstallPixels(info, (IntPtr)pData, image.Stride);
                }
                return bmp;
            }
        }

        private readonly Lazy<SKBitmap[]> _tiles;
        public IReadOnlyList<SKBitmap> Tiles => _tiles.Value;

        public Sprite(string texturePath, int frameCount) => _tiles = new Lazy<SKBitmap[]>(() => Load(texturePath, frameCount));

        private SKBitmap[] Load(string texturePath, int frameCount)
        {
            if (string.IsNullOrEmpty(texturePath))
                throw new ArgumentNullException(texturePath);

            if (frameCount <= 0)
                throw new ArgumentException("Invalid frame count", nameof(frameCount));

            using(var texture = LoadFile(texturePath))
            {
                return LoadTiles(texture, frameCount).ToArray();
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (disposedValue)
                return;

            if (disposing && _tiles.IsValueCreated)
            {
                foreach (var tile in _tiles.Value)
                {
                    tile.Dispose();
                }
            }

            disposedValue = true;
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}