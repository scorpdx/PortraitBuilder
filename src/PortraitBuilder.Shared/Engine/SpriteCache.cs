using Microsoft.Extensions.Logging;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace PortraitBuilder.Engine
{
    public sealed class SpriteCache : IDisposable, ISpriteCache
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<SpriteCache>();

        public IReadOnlyList<Content> ActiveContent { get; }

        private ConcurrentDictionary<SpriteDef, Sprite> _sprites;

        public SpriteCache(IEnumerable<Content> activeContent)
        {
            //create a copy
            ActiveContent = new List<Content>(activeContent);
            _sprites = new ConcurrentDictionary<SpriteDef, Sprite>();
        }

        public Sprite Get(SpriteDef def) => _sprites?.GetOrAdd(def, LoadSprite) ?? throw new ObjectDisposedException(nameof(SpriteCache));

        private Sprite LoadSprite(SpriteDef def)
        {
            // Paths in vanilla files are Windows-style
            string filePath = def.TextureFilePath;//.Replace('\\', Path.DirectorySeparatorChar);

            // Also try alternative extension (.tga <=> .dds)
            string extension = Path.GetExtension(filePath);
            string alternative = string.Equals(extension, ".dds", StringComparison.OrdinalIgnoreCase) ? ".tga" : ".dds";
            string alternativeFilePath = Path.ChangeExtension(filePath, alternative);

            string path = null;

            // Loop on reverse order - last occurence wins if asset is overriden !
            for (int i = ActiveContent.Count - 1; i >= 0; i--)
            {
                var content = ActiveContent[i];
                path = Path.Combine(content.AbsolutePath, filePath);

                if (!File.Exists(path))
                {
                    path = Path.Combine(content.AbsolutePath, alternativeFilePath);
                }

                if (File.Exists(path))
                {
                    break;
                }
            }

            if (string.IsNullOrEmpty(path))
                throw new FileNotFoundException(string.Format("Unable to find file: {0} under active content {1}", filePath, ActiveContent));

            logger.LogDebug("Loading sprite from: {0}", path);
            return new Sprite(path, def.FrameCount);
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
