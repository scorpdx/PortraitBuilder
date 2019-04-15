using PortraitBuilder.Engine;

namespace PortraitBuilder.ContentPacks
{

    /// <summary>
    /// Loads content based on hierachical override: vanilla -> DLC -> mod -> dependent mod
    /// </summary>
    public class PackLoader : Loader
    {
        public override void InvalidateCache()
        {
            var oldcache = Cache;
            Cache = new PackSpriteCache(ActiveContent);

            oldcache?.Dispose();
        }
    }
}
