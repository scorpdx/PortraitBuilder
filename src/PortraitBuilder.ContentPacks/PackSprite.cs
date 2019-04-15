using PortraitBuilder.Engine;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PortraitBuilder.ContentPacks
{
    public class PackSprite : ISprite
    {
        private readonly Lazy<SKBitmap>[] _tiles;

        public PackSprite(string tileDir, int frameCount)
        {
            var dir = new DirectoryInfo(tileDir);
            var foundTiles = dir.EnumerateFiles("*.png")
                .Select(fi => new
                {
                    Index = int.Parse(Path.GetFileNameWithoutExtension(fi.Name)),
                    Tile = new Lazy<SKBitmap>(() => SKBitmap.Decode(fi.FullName))
                });

            _tiles = new Lazy<SKBitmap>[frameCount];
            foreach (var a in foundTiles)
            {
                _tiles[a.Index] = a.Tile;
            }
        }

        public SKBitmap this[int index] => _tiles[index].Value;

        public int Count => _tiles.Length;

        public void Dispose() => Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var tile in _tiles.Where(tile => tile?.IsValueCreated ?? false))
                {
                    tile.Value.Dispose();
                }
            }
        }

        public IEnumerator<SKBitmap> GetEnumerator() => ((IEnumerable<Lazy<SKBitmap>>)_tiles).Select(_tile => _tile.Value).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _tiles.GetEnumerator();
    }
}
