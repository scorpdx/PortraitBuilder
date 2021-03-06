﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using PortraitBuilder.Model.Content;

namespace PortraitBuilder.Parser
{
    using static EncodingHelper;
    public static class ModReader
    {

        private static readonly ILogger logger = LoggingHelper.CreateLogger<Mod>();

        /// <summary>
        /// Loads all files in the given folder.
        /// </summary>
        /// <param name="folder">Path to the folder containing the files to load.</param>
        public static List<Mod> ParseFolder(string folder)
        {
            List<Mod> mods = new List<Mod>();

            DirectoryInfo dir = new DirectoryInfo(folder);
            if (dir.Exists)
            {
                var modFiles = dir.EnumerateFiles("*.mod");
                if (!modFiles.Any())
                {
                    logger.LogWarning("No mods found in folder: {0}", dir.FullName);
                }

                foreach (FileInfo modFile in modFiles)
                {
                    try
                    {
                        Mod mod = Parse(modFile.FullName);
                        if (mod?.ModPath != null)
                        {
                            mod.AbsolutePath = Path.Combine(folder, mod.ModPath.Substring("mod".Length + 1)); // Remove "mod/" from path
                            mods.Add(mod);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Could not parse .mod file: " + modFile, e);
                    }
                }
            }
            else
            {
                logger.LogError(string.Format("Folder not found: {0}", dir.FullName));
            }

            return mods;
        }

        private static Mod Parse(string filename)
        {
            if (!File.Exists(filename))
            {
                logger.LogError(string.Format("File not found: {0}", filename));
                return null;
            }

            string line;
            FileInfo modFile = new FileInfo(filename);
            Mod mod = new Mod();
            mod.ModFile = modFile.Name;

            StreamReader reader = new StreamReader(filename, WesternEncoding);
            while ((line = reader.ReadLine()) != null)
            {
                if (line.StartsWith("#"))
                    continue;

                if (line.StartsWith("name"))
                    mod.Name = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();
                if (line.StartsWith("path") || line.StartsWith("archive"))
                    mod.ModPath = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();
                if (line.StartsWith("user_dir") || line.StartsWith("archive"))
                    mod.UserDir = line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim();

                if (line.StartsWith("extend"))
                    mod.Extends.Add(line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim());
                if (line.StartsWith("replace"))
                    mod.Replaces.Add(line.Split('=')[1].Split('#')[0].Replace("\"", "").Trim().Replace('/', '\\'));

                if (line.StartsWith("dependencies"))
                {
                    string[] dependencies = line.Split('=')[1].Split('#')[0].Replace("{", "").Replace("}", "").Trim().Split('"');
                    foreach (string dependency in dependencies)
                    {
                        if (dependency.Trim() != string.Empty)
                        {
                            mod.Dependencies.Add(dependency);
                        }
                    }
                }
            }
            return mod;
        }
    }
}
