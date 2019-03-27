using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using PortraitBuilder.Model.Content;

namespace PortraitBuilder.Parser
{
    using static EncodingHelper;
    public class DLCReader
    {

        private static readonly ILogger logger = LoggingHelper.CreateLogger<DLC>();

        public IEnumerable<DLC> ParseFolder(string folder)
        {
            DirectoryInfo dir = new DirectoryInfo(folder);
            if (!dir.Exists)
            {
                logger.LogError(string.Format("Folder not found: {0}", dir.FullName));
                yield break;
            }

            var dlcFiles = dir.EnumerateFiles("*.dlc");
            if (!dlcFiles.Any())
            {
                logger.LogError("No DLC files found in folder: {0}", dir.FullName);
                yield break;
            }

            var parsedDlcs = dlcFiles
                .Select(fi => Parse(fi.FullName))
                .Where(dlc => dlc?.Archive != null);
            foreach (var dlc in parsedDlcs)
            {
                // Note: path will be overriden when extracting the archive
                // Remove "dlc/" from path
                dlc.AbsolutePath = Path.Combine(folder, dlc.Archive.Substring("dlc/".Length));
                yield return dlc;
            }
        }

        private DLC Parse(string filename)
        {
            var dlcFile = new FileInfo(filename);
            Debug.Assert(dlcFile.Exists);

            var dlc = new DLC(dlcFile.Name);
            foreach(var line in File.ReadLines(filename, WesternEncoding).Where(l => !l.StartsWith("#")))
            {
                if (line.StartsWith("name"))
                    dlc.Name = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();
                if (line.StartsWith("archive"))
                    dlc.Archive = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();
                if (line.StartsWith("checksum"))
                    dlc.Checksum = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();

                if (line.StartsWith("steam_id"))
                {
                    if (int.TryParse(line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim(), out int intOut))
                        dlc.SteamID = intOut;
                    else
                        logger.LogError(string.Format("Error parsing Steam ID in file: {0}", dlcFile.Name));
                }

                if (line.StartsWith("gamersgate_id"))
                {
                    if (int.TryParse(line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim(), out int intOut))
                        dlc.GamersGateID = intOut;
                    else
                        logger.LogError(string.Format("Error parsing GamersGate ID in file: {0}", dlcFile.Name));
                }

                if (line.StartsWith("affects_checksum"))
                    dlc.AffectsChecksum = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim() == "yes";
            }
            return dlc;
        }
    }
}
