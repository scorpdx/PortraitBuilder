using Microsoft.Extensions.Logging;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;
using System.Collections.Generic;
using System.Linq;

namespace PortraitBuilder.Engine
{
    public abstract class Loader
    {
        private static readonly ILogger logger = LoggingHelper.CreateLogger<Loader>();

        /// <summary>
        /// DLCs or Mods that are checked
        /// </summary>
        public IList<Content> ActiveContent { get; protected set; } = new List<Content>();

        public virtual ISpriteCache Cache { get; protected set; }

        /// <summary>
        /// Merged portraitData of all active content.
        /// </summary>
        public PortraitData ActivePortraitData { get; protected set; } = new PortraitData();

        public PortraitType GetPortraitType(string basePortraitType)
            => ActivePortraitData.PortraitTypes[basePortraitType];

        public PortraitType GetPortraitType(string basePortraitType, string clothingPortraitType)
            => ActivePortraitData.PortraitTypes[basePortraitType].Merge(ActivePortraitData.PortraitTypes[clothingPortraitType]);

        public abstract void InvalidateCache();

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