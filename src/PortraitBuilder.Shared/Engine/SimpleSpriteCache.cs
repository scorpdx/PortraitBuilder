using PortraitBuilder.Model.Portrait;
using System;
using System.Collections.Concurrent;

namespace PortraitBuilder.Engine
{
    public class SimpleSpriteCache<T> : ISpriteCache where T : ISprite
    {
        private readonly Func<SpriteDef, T> _resolver;

        private readonly ConcurrentDictionary<SpriteDef, T> _sprites = new ConcurrentDictionary<SpriteDef, T>();

        public SimpleSpriteCache(Func<SpriteDef, T> resolver)
        {
            _resolver = resolver;
        }

        public T Get(SpriteDef def)
        {
            if (disposedValue)
                throw new ObjectDisposedException(nameof(SimpleSpriteCache<T>));

            return _sprites.GetOrAdd(def, _resolver);
        }

        ISprite ISpriteCache.Get(SpriteDef def) => Get(def);

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
