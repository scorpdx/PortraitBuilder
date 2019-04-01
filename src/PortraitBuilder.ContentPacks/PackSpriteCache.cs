using Microsoft.Extensions.Logging;
using PortraitBuilder.Engine;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PortraitBuilder.ContentPacks
{
    public class PackSpriteCache : ISpriteCache
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<PackSpriteCache>();

        public IReadOnlyList<Content> ActiveContent { get; }

        private ConcurrentDictionary<SpriteDef, Sprite> _sprites;

        public PackSpriteCache(IEnumerable<Content> activeContent)
        {
            //create a copy
            ActiveContent = new List<Content>(activeContent);
            _sprites = new ConcurrentDictionary<SpriteDef, Sprite>();
        }

        public Sprite Get(SpriteDef def) => _sprites?.GetOrAdd(def, LoadSprite) ?? throw new ObjectDisposedException(nameof(PackSpriteCache));

        private PackSprite LoadSprite(SpriteDef def)
        {
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
                    _sprites = null;

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
