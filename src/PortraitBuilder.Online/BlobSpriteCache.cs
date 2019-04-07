using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using PortraitBuilder.Engine;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PortraitBuilder.Online
{
    public class BlobSpriteCache : ISpriteCache
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<BlobSpriteCache>();

        private readonly ConcurrentDictionary<SpriteDef, BlobSprite> _sprites;

        private readonly CloudBlobContainer _pack;

        public BlobSpriteCache(CloudBlobContainer pack)
        {
            _pack = pack;
            _sprites = new ConcurrentDictionary<SpriteDef, BlobSprite>();
        }

        public BlobSprite Get(SpriteDef def) => _sprites?.GetOrAdd(def, LoadSprite) ?? throw new ObjectDisposedException(nameof(BlobSpriteCache));
        ISprite ISpriteCache.Get(SpriteDef def) => Get(def);

        private Task<BlobSprite> LoadSpriteAsync(SpriteDef def)
        {
            var blobPath = def.TextureFilePath;
            if (string.IsNullOrEmpty(blobPath))
                throw new ArgumentException($"No texture path in sprite definition {def}", nameof(def));

            var blob = _pack.GetBlockBlobReference(blobPath);
            blob.
            DirectoryInfo dir = null;
            // Loop on reverse order - last occurence wins if asset is overriden !
            for (int i = ActiveContent.Count - 1; i >= 0; i--)
            {
                var content = ActiveContent[i];
                dir = new DirectoryInfo(Path.Combine(content.AbsolutePath, def.Name));

                if(dir.Exists && dir.EnumerateFiles().Any()) break;
            }

            if (dir == null || !dir.Exists)
                throw new FileNotFoundException(string.Format("Unable to find sprite: {0} under active content {1}", def.Name, ActiveContent));

            logger.LogDebug("Loading pack sprite from: {0}", dir.FullName);
            return new PackSprite(dir.FullName, def.FrameCount);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    var oldDict = _sprites;
                    foreach (var sprite in oldDict.Values)
                    {
                        sprite.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}
