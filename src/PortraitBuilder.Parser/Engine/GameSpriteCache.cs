using Microsoft.Extensions.Logging;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace PortraitBuilder.Engine
{
    public sealed class GameSpriteCache : IDisposable, ISpriteCache
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<GameSpriteCache>();

        public IReadOnlyList<Content> ActiveContent { get; }

        private readonly ConcurrentDictionary<SpriteDef, GameSprite> _sprites;

        public GameSpriteCache(IEnumerable<Content> activeContent)
        {
            //create a copy
            ActiveContent = new List<Content>(activeContent);
            _sprites = new ConcurrentDictionary<SpriteDef, GameSprite>();
        }

        public GameSprite Get(SpriteDef def)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(GameSpriteCache));

            return _sprites.GetOrAdd(def, LoadSprite);
        }

        ISprite ISpriteCache.Get(SpriteDef def) => Get(def);

        private GameSprite? LoadSprite(SpriteDef def)
        {
            bool found = false;

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
                    found = true;
                    break;
                }
            }

            if (!found)
            {
                logger.LogWarning("Unable to find file: {0} under active content {1}", filePath, ActiveContent);
                return null;
            }

            logger.LogDebug("Loading sprite from: {0}", path);
            return new GameSprite(path, def.FrameCount);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    foreach (var sprite in _sprites.Values)
                    {
                        sprite?.Dispose();
                    }
                }

                disposedValue = true;
            }
        }

        public void Dispose() => Dispose(true);
        #endregion
    }
}
