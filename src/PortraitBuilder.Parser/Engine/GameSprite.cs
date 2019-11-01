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
        private static SKBitmap[] LoadTiles(SKBitmap texture, int frameCount)
        {
            var src = new SKRectI(0, 0, texture.Width / frameCount, texture.Height);
            var tileInfo = texture.Info.WithSize(src.Width, src.Height);
            Debug.Assert(tileInfo.RowBytes == src.Width * texture.Info.BytesPerPixel);

            var ret = new SKBitmap[frameCount];
            for (int i = 0; i < frameCount; i++, src.Offset(src.Width, 0))
            {
                var tile = new SKBitmap(tileInfo);
                if (!texture.ExtractSubset(tile, src))
                    throw new InvalidOperationException($"Failed to extract tile {i} from texture {texture}");

                tile.SetImmutable();
                ret[i] = tile;
            }

            return ret;
        }

        private static SKBitmap LoadFile(string filepath)
        {
            using var image = Pfim.Pfim.FromFile(filepath);
            if (image.Format != Pfim.ImageFormat.Rgba32 || image.Compressed)
                throw new InvalidOperationException("Unexpected image format");

            var info = new SKImageInfo(image.Width, image.Height, SKColorType.Bgra8888, SKAlphaType.Unpremul);

            unsafe
            {
                fixed (byte* pData = image.Data)
                {
                    using var bmp = new SKBitmap(info);
                    if (!bmp.InstallPixels(info, (IntPtr)pData, image.Stride))
                        throw new InvalidOperationException("Failed to load pixmap content");

                    //Copying the bitmap seems to be the only reliable way to avoid
                    //AccessViolationExceptions in Skia, even though we created it above
                    return bmp.Copy();
                }
            }
        }

        private readonly Lazy<SKBitmap[]> _tiles;

        public int Count { get; }

        public SKBitmap this[int index] => _tiles.Value[index];

        public GameSprite(string texturePath, int frameCount)
        {
            Count = frameCount;
            _tiles = new Lazy<SKBitmap[]>(() => Load(texturePath, frameCount), System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        private SKBitmap[] Load(string texturePath, int frameCount)
        {
            if (string.IsNullOrEmpty(texturePath))
                throw new ArgumentNullException(texturePath);

            if (frameCount <= 0)
                return Array.Empty<SKBitmap>();

            using var texture = LoadFile(texturePath);
            return LoadTiles(texture, frameCount);
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