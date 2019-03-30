using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using PortraitBuilder.Model;
using PortraitBuilder.Parser;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;
using Microsoft.Extensions.Logging;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace PortraitBuilder.Engine
{

    /// <summary>
    /// Loads content based on hierachical override: vanilla -> DLC -> mod -> dependent mod
    /// </summary>
    public class Loader
    {

        private static readonly ILogger logger = LoggingHelper.CreateLogger<Loader>();

        /// <summary>
        /// DLCs or Mods that are checked
        /// </summary>
        public List<Content> ActiveContents { get; } = new List<Content>();

        public SpriteCache Cache { get; private set; }

        /// <summary>
        /// Merged portraitData of all active content.
        /// </summary>
        public PortraitData ActivePortraitData { get; private set; } = new PortraitData();

        /// <summary>
        /// Vanilla data - never reloaded dynamically
        /// </summary>
        public Content Vanilla { get; private set; }

        public PortraitType GetPortraitType(string basePortraitType)
        {
            return ActivePortraitData.PortraitTypes[basePortraitType];
        }

        public PortraitType GetPortraitType(string basePortraitType, string clothingPortraitType)
            => ActivePortraitData.PortraitTypes[basePortraitType].Merge(ActivePortraitData.PortraitTypes[clothingPortraitType]);

        public void LoadVanilla(string gameDir)
        {
            Vanilla = new Content();
            Vanilla.Name = "vanilla";
            Vanilla.AbsolutePath = gameDir;

            logger.LogInformation("Loading portraits from vanilla.");
            var reader = new PortraitReader(gameDir);
            Vanilla.PortraitData = reader.Parse();

            // Init
            ActivePortraitData = Vanilla.PortraitData;
            ActiveContents.Add(Vanilla);
            InvalidateCache();
        }

        public List<DLC> LoadDLCs(string gameDir, string dlcDir, bool clean = false)
        {
            if (clean)
            {
                // Cleanup temporary DLC Dir
                Directory.Delete(dlcDir, true);
            }
            return LoadDLCs(gameDir, dlcDir).ToList();
        }

        public IEnumerable<DLC> LoadDLCs(string gameDir, string dlcDir)
        {
            string dlcFolder = Path.Combine(gameDir, "DLC");
            logger.LogInformation("Loading DLCs from " + dlcFolder);

            foreach (DLC dlc in DLCReader.ParseFolder(dlcFolder))
            {
                UnzipDLC(dlcDir, dlc);

                logger.LogInformation("Loading portraits from DLC: " + dlc.Name);
                var reader = new PortraitReader(dlc.AbsolutePath);
                dlc.PortraitData = reader.Parse();

                yield return dlc;
            }
        }

        private static Regex[] DlcEntryFilters { get; } = "interface;gfx/characters"
            .Split(';')
            .Select(f => new Regex(f, RegexOptions.IgnoreCase))
            .ToArray();

        /// <summary>
        /// Unzip DLC, only if tmp folder doesn't already exist
        /// </summary>
        /// <param name="dlcs"></param>
        private void UnzipDLC(string dlcDir, DLC dlc)
        {
            string dlcCode = dlc.DLCFile.Replace(".dlc", "");
            string newDlcAbsolutePath = Path.Combine(dlcDir, dlcCode);
            if (!Directory.Exists(newDlcAbsolutePath))
            {
                logger.LogInformation(string.Format("Extracting {0} to {1}", dlc.Name, newDlcAbsolutePath));
                // Filter only portraits files, to gain speed/space
                using (var zip = ZipFile.OpenRead(dlc.AbsolutePath))
                {
                    var filteredEntries = zip.Entries
                        .Where(e => e.Length > 0)
                        .Where(e => DlcEntryFilters.Any(r => r.IsMatch(e.FullName)));
                    foreach (var entry in filteredEntries)
                    {
                        var fi = new FileInfo(Path.Combine(newDlcAbsolutePath, entry.FullName));
                        fi.Directory.CreateSubdirectory(".");

                        entry.ExtractToFile(fi.FullName);
                    }
                }
                //ZipFile.ExtractToDirectory(dlc.AbsolutePath, newDlcAbsolutePath, fileFilter);

                // In any case, create the directory, so that it is ignored for next load.
                Directory.CreateDirectory(newDlcAbsolutePath);
            }
            dlc.AbsolutePath = newDlcAbsolutePath;
        }

        public List<Mod> LoadMods(string modDir)
        {
            List<Mod> mods = new List<Mod>();
            if (Directory.Exists(modDir))
            {
                logger.LogInformation("Loading mods from " + modDir);
                mods = ModReader.ParseFolder(modDir);
                foreach (Mod mod in mods)
                {
                    if (Directory.Exists(mod.AbsolutePath))
                    {
                        logger.LogInformation("Loading portraits from mod: " + mod.Name);
                        var reader = new PortraitReader(mod.AbsolutePath);
                        mod.PortraitData = reader.Parse();

                        if (!mod.HasPortraitData)
                        {
                            mod.Enabled = false;
                            mod.DisabledReason = "No portrait data found";
                        }
                    }
                    else if (mod.AbsolutePath.EndsWith(".zip"))
                    {
                        mod.Enabled = false;
                        mod.DisabledReason = "Archive format is not supported by PortraitBuilder";
                        logger.LogWarning("Mod " + mod.Name + " is using archive format, which is not supported by PortraitBuilder");
                    }
                    else
                    {
                        mod.Enabled = false;
                        mod.DisabledReason = "Mod path does not not exist";
                        logger.LogError("Mod path " + mod.AbsolutePath + " does not exist");
                    }
                }
            }
            else
            {
                logger.LogError("Mod directory {0} doesn't exist", modDir);
            }

            return mods;
        }

        public void ActivateContent(Content content)
        {
            // TODO load order
            ActiveContents.Add(content);
            InvalidateCache();

            RefreshContent(content);
        }

        public void DeactivateContent(Content content)
        {
            ActiveContents.Remove(content);
            InvalidateCache();
        }

        public void UpdateActiveAdditionalContent(IReadOnlyCollection<Content> contents)
        {
            ActiveContents.Clear();
            ActiveContents.Add(Vanilla);
            ActiveContents.AddRange(contents);
            InvalidateCache();
        }

        private void InvalidateCache()
        {
            Cache?.Dispose();
            Cache = new SpriteCache(ActiveContents);
        }

        public void RefreshContent(Content content)
        {
            logger.LogInformation("Refreshing content: " + content.Name);

            var reader = new PortraitReader(content.AbsolutePath);
            content.PortraitData = reader.Parse();

            LoadPortraits();
        }

        private void MergePortraitData()
        {
            ActivePortraitData = new PortraitData();

            ActivePortraitData.MergeWith(Vanilla.PortraitData);
            // Recalculate merged portrait data
            foreach (Content content in ActiveContents)
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
