using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PortraitBuilder.Engine
{
    public sealed class GameSprite : ISprite
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
            if (image.Format != Pfim.ImageFormat.Rgba32 || image.Compressed)
                throw new InvalidOperationException("Unexpected image format");

            Debug.Assert(image.Format == Pfim.ImageFormat.Rgba32);
            Debug.Assert(image.Compressed == false);

            var info = new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            var bmp = new SKBitmap(info);
            unsafe
            {
                fixed (byte* pData = image.Data)
                {
                    using (var map = new SKPixmap(info, (IntPtr)pData, image.Stride))
                    {
                        if (!bmp.InstallPixels(map))
                            throw new InvalidOperationException("Failed to load pixmap content");
                    }
                }
            }
            return bmp;
        }

        private readonly Lazy<SKBitmap[]> _tiles;

        public int Count { get; }

        public SKBitmap this[int index] => _tiles.Value[index];

        public GameSprite(string texturePath, int frameCount)
        {
            Count = frameCount;
            _tiles = new Lazy<SKBitmap[]>(() => frameCount <= 0 ? Array.Empty<SKBitmap>() : Load(texturePath, frameCount));
        }

        private SKBitmap[] Load(string texturePath, int frameCount)
        {
            if (string.IsNullOrEmpty(texturePath))
                throw new ArgumentNullException(texturePath);

            if (frameCount <= 0)
                throw new ArgumentException("Invalid frame count", nameof(frameCount));

            using (var texture = LoadFile(texturePath))
            {
                return LoadTiles(texture, frameCount).ToArray();
            }
        }

        public IEnumerator<SKBitmap> GetEnumerator() => ((IEnumerable<SKBitmap>)_tiles.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _tiles.Value.GetEnumerator();

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public void Dispose()
        {
            if (disposedValue)
                return;

            if (_tiles.IsValueCreated)
            {
                foreach (var tile in _tiles.Value)
                {
                    tile.Dispose();
                }
            }

            disposedValue = true;
        }
        #endregion
    }
}