using Microsoft.Extensions.Logging;
using PortraitBuilder.Engine;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PortraitBuilder.ContentPacks
{
    public class PackSprite : Sprite
    {
        private static ILogger logger = LoggingHelper.CreateLogger<PackSprite>();

        public override IReadOnlyList<SKBitmap> Tiles { get; }

        public PackSprite(string tileDir, int frameCount)
        {
            var dir = new DirectoryInfo(tileDir);
            var foundTiles = dir.EnumerateFiles("*.png")
                .Select(fi => new
                {
                    Index = int.Parse(Path.GetFileNameWithoutExtension(fi.Name)),
                    Tile = SKBitmap.Decode(fi.FullName)
                });

            var tiles = new SKBitmap[frameCount];
            foreach (var a in foundTiles)
            {
                tiles[a.Index] = a.Tile;
            }

            if (tiles.Any(a => a == null))
            {
                logger.LogWarning("Pack sprite in {0} expected {1} frames but got {2}", dir.FullName, frameCount, tiles.Count(a => a != null));
            }

            Tiles = tiles;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var tile in Tiles)
                {
                    tile?.Dispose();
                }
            }
        }
    }
}
