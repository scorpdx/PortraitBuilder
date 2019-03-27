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
        /// User configuration: game path, etc.
        /// </summary>
        private User user;

        /// <summary>
        /// Stateless mod scanner
        /// </summary>
        private ModReader modReader = new ModReader();

        /// <summary>
        /// Stateless dlc scanner
        /// </summary>
        private DLCReader dlcReader = new DLCReader();

        /// <summary>
        /// DLCs or Mods that are checked
        /// </summary>
        public List<Content> ActiveContents { get; } = new List<Content>();

        /// <summary>
        /// Merged portraitData of all active content.
        /// </summary>
        public PortraitData ActivePortraitData { get; private set; } = new PortraitData();

        /// <summary>
        /// Vanilla data - never reloaded dynamically
        /// </summary>
        private Content vanilla;

        public Loader(User user)
        {
            this.user = user;
        }

        public PortraitType GetPortraitType(string basePortraitType)
        {
            return ActivePortraitData.PortraitTypes[basePortraitType];
        }

        public PortraitType GetPortraitType(string basePortraitType, string clothingPortraitType)
            => ActivePortraitData.PortraitTypes[basePortraitType].Merge(ActivePortraitData.PortraitTypes[clothingPortraitType]);

        public void LoadVanilla()
        {
            vanilla = new Content();
            vanilla.Name = "vanilla";
            vanilla.AbsolutePath = user.GameDir;

            logger.LogInformation("Loading portraits from vanilla.");
            var reader = new PortraitReader(user.GameDir);
            vanilla.PortraitData = reader.Parse();

            // Init
            ActivePortraitData = vanilla.PortraitData;
            ActiveContents.Add(vanilla);
        }

        public List<DLC> LoadDLCs(Boolean clean)
        {
            if (clean)
            {
                // Cleanup temporary DLC Dir
                Directory.Delete(user.DlcDir, true);
            }
            return LoadDLCs().ToList();
        }

        public IEnumerable<DLC> LoadDLCs()
        {
            string dlcFolder = Path.Combine(user.GameDir, "DLC");
            logger.LogInformation("Loading DLCs from " + dlcFolder);

            foreach (DLC dlc in dlcReader.ParseFolder(dlcFolder))
            {
                UnzipDLC(dlc);

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
        private void UnzipDLC(DLC dlc)
        {
            string dlcCode = dlc.DLCFile.Replace(".dlc", "");
            string newDlcAbsolutePath = Path.Combine(user.DlcDir, dlcCode);
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

        public List<Mod> LoadMods()
        {
            List<Mod> mods = new List<Mod>();
            if (Directory.Exists(user.ModDir))
            {
                logger.LogInformation("Loading mods from " + user.ModDir);
                mods = modReader.ParseFolder(user.ModDir);
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
                logger.LogError("Mod directory " + user.ModDir + " doesn't exist");
            }

            return mods;
        }

        public void ActivateContent(Content content)
        {
            // TODO load order
            ActiveContents.Add(content);
            RefreshContent(content);
        }

        public void DeactivateContent(Content content)
        {
            ActiveContents.Remove(content);
            content.Unload();
        }

        public void UpdateActiveAdditionalContent(IReadOnlyCollection<Content> contents)
        {
            foreach (var content in ActiveContents)
            {
                if (!contents.Contains(content))
                {
                    //Unload sprites
                    content.Unload();
                }
            }

            ActiveContents.Clear();
            ActiveContents.Add(vanilla);
            ActiveContents.AddRange(contents);
        }

        public void RefreshContent(Content content)
        {
            logger.LogInformation("Refreshing content: " + content.Name);
            content.Unload();

            var reader = new PortraitReader(content.AbsolutePath);
            content.PortraitData = reader.Parse();

            LoadPortraits();
        }

        private void MergePortraitData()
        {
            ActivePortraitData = new PortraitData();

            ActivePortraitData.MergeWith(vanilla.PortraitData);
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
