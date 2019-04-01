using System.Collections.Generic;
using System.Linq;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;
using Microsoft.Extensions.Logging;

namespace PortraitBuilder.ContentPacks
{

    /// <summary>
    /// Loads content based on hierachical override: vanilla -> DLC -> mod -> dependent mod
    /// </summary>
    public class PackLoader
    {

        private static readonly ILogger logger = LoggingHelper.CreateLogger<PackLoader>();

        /// <summary>
        /// DLCs or Mods that are checked
        /// </summary>
        public List<Content> ActiveContent { get; } = new List<Content>();

        public PackSpriteCache Cache { get; private set; }

        /// <summary>
        /// Merged portraitData of all active content.
        /// </summary>
        public PortraitData ActivePortraitData { get; private set; } = new PortraitData();

        public PortraitType GetPortraitType(string basePortraitType)
            => ActivePortraitData.PortraitTypes[basePortraitType];

        public PortraitType GetPortraitType(string basePortraitType, string clothingPortraitType)
            => ActivePortraitData.PortraitTypes[basePortraitType].Merge(ActivePortraitData.PortraitTypes[clothingPortraitType]);

        public void InvalidateCache()
        {
            var oldcache = Cache;
            Cache = new PackSpriteCache(ActiveContent);

            oldcache?.Dispose();
        }

        private void MergePortraitData()
        {
            ActivePortraitData = new PortraitData();
            foreach (Content content in ActiveContent)
            {
                ActivePortraitData.MergeWith(content.PortraitData);
            }
        }

        public void LoadPortraits()
        {
            MergePortraitData();

            // Apply external offsets
            var allLayers = ActivePortraitData.PortraitTypes.Values
                .SelectMany(pt => pt.Layers.Where(layer => ActivePortraitData.Offsets.ContainsKey(layer.Name)));
            foreach (var layer in allLayers)
            {
                layer.Offset = ActivePortraitData.Offsets[layer.Name];
                logger.LogDebug("Overriding offset of layer {0} to {1}", layer.Name, layer.Offset);
            }
        }
    }
}
