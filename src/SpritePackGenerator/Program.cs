using PortraitBuilder.ContentPacks;
using PortraitBuilder.Engine;
using PortraitBuilder.Model;
using PortraitBuilder.Model.Content;
using PortraitBuilder.Model.Portrait;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SpritePackGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            if (!args.Any() || args.Length != 2)
            {
                Console.WriteLine("Arguments: <gamedir> <moddir>");
                Console.WriteLine("Example:");
                var game = @"""C:\Program Files (x86)\Steam\steamapps\common\Crusader Kings II""";
                var mods = @"""C:\Users\MyUser\Documents\Paradox Interactive\Crusader Kings II""";
                Console.WriteLine("{0} {1}", game, mods);
                return;
            }

            var user = new User
            {
                GameDir = args[0],
                ModDir = args[1],
                DlcDir = "dlc/"
            };

            {
                var loader = new GameLoader();
                Console.WriteLine("Loading vanilla content from {0}", user.GameDir);
                loader.LoadVanilla(user.GameDir);
                ExtractContentSprites(loader.Vanilla, false);
                ExtractContent(loader.Vanilla, false);

                Console.WriteLine("Loading DLC content from {0}", user.ModDir);
                Console.WriteLine("Saving to {0}", user.DlcDir);
                var activeDlcs = new List<Content>();
                foreach (Content dlc in loader.LoadDLCs(user.GameDir, user.DlcDir))
                {
                    if (dlc.HasPortraitData)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        activeDlcs.Add(dlc);
                    }
                    Console.WriteLine(" [{1,3}]\t{0}", dlc.Name, dlc.PortraitData.Sprites.Count);
                    Console.ResetColor();
                }
                Console.WriteLine();
                Console.WriteLine("{0} active DLCs", activeDlcs.Count);

                foreach (var dlc in activeDlcs)
                {
                    ExtractContentSprites(dlc, true);
                    ExtractContent(dlc, true);
                }

                Console.WriteLine("Extracted packs.");
            }
            {
                Console.WriteLine("Generating mergefile...");

                var portraitData = MergePacks();

                var options = new JsonSerializerOptions();
                options.Converters.Add(new SkiaConverter());
                File.WriteAllText("packs/portraits.json", JsonSerializer.Serialize(portraitData, options));

                Console.WriteLine("Saved portrait mergefile.");
                Console.WriteLine("Done");
            }
        }

        private static PortraitData MergePacks()
        {
            var packDir = new DirectoryInfo("packs/");

            var options = new JsonSerializerOptions();
            options.Converters.Add(new SkiaConverter());

            var packContents = packDir.EnumerateFiles("*.json", SearchOption.AllDirectories)
                .Where(fi => fi.DirectoryName != packDir.FullName.TrimEnd(Path.DirectorySeparatorChar))
                .Select(fi => fi.FullName)
                .Select(File.ReadAllText)
                .Select(json => JsonSerializer.Deserialize<Content>(json, options));

            var loader = new PackLoader();
            foreach (var pack in packContents)
            {
                Console.WriteLine("Loaded pack {0}", pack.Name);
                loader.ActiveContent.Add(pack);
            }
            loader.LoadPortraits();
            loader.InvalidateCache();

            var activeContent = loader.ActiveContent;
            foreach (var kvp in loader.ActivePortraitData.Sprites)
            {
                var def = kvp.Value;

                string originalPath = null;
                DirectoryInfo dir = null;
                for (int i = activeContent.Count - 1; i >= 0; i--)
                {
                    var content = activeContent[i];
                    originalPath = Path.Combine(content.AbsolutePath, def.Name).Replace('\\', '/');
                    dir = new DirectoryInfo(originalPath);

                    if (dir.Exists && dir.EnumerateFiles().Any()) break;
                }

                if (dir == null || !dir.Exists)
                {
                    Console.Error.WriteLine("Unable to find sprite: {0} with {1} active content packs", def.Name, activeContent.Count);
                    def.TextureFilePath = null;
                    continue;
                }

                Debug.Assert(originalPath != null);
                Console.WriteLine("Resolved pack sprite {0} from: {1}", kvp.Key, originalPath);

                def.TextureFilePath = originalPath;
            }

            foreach (var badKey in loader.ActivePortraitData.Sprites.Where(kvp => kvp.Value.TextureFilePath == null).Select(kvp => kvp.Key))
            {
                loader.ActivePortraitData.Sprites.Remove(badKey);
                Console.WriteLine("Removed missing sprite {0}", badKey);
            }

            return loader.ActivePortraitData;
        }

        private static void ExtractContent(Content content, bool dlc)
        {
            var definitionPath = Path.Combine("packs", dlc ? content.AbsolutePath : "vanilla");

            var definitionJsonPath = Path.Combine(definitionPath, Path.ChangeExtension(content.Name, ".json"));
            var jsonFile = new FileInfo(definitionJsonPath);
            jsonFile.Directory.Create();

            if (jsonFile.Exists)
                return;

            content.AbsolutePath = Path.Combine(definitionPath, "tiles/");

            var options = new JsonSerializerOptions();
            options.Converters.Add(new SkiaConverter());
            File.WriteAllText(definitionJsonPath, JsonSerializer.Serialize(content, options));
        }

        private static void ExtractContentSprites(Content content, bool dlc)
        {
            using var cache = new GameSpriteCache(Enumerable.Repeat(content, 1));
            foreach (var def in content.PortraitData.Sprites.Values)
            {
                Console.WriteLine("Sprite {0}", def.Name);
                var spritePath = Path.Combine("packs", dlc ? content.AbsolutePath : "vanilla", "tiles/", def.Name + "/");
                var spriteDir = new DirectoryInfo(spritePath);

                try
                {
                    using var sprite = cache.Get(def);

                    if (sprite.Any())
                        spriteDir.Create();

                    Console.Write("[{0}]", new string(' ', sprite.Count));
                    Console.Write(new string('\b', sprite.Count + 1));
                    for (int i = 0; i < sprite.Count; i++)
                    {
                        var tile = sprite[i];
                        var tilePath = Path.Combine(spritePath, $"{i}.png");
                        if (File.Exists(tilePath))
                        {
                            Console.Write('-');
                            continue;
                        }

                        using (var fs = File.Create(tilePath))
                        {
                            var pixmap = tile.PeekPixels();
                            pixmap.Encode(SkiaSharp.SKPngEncoderOptions.Default).SaveTo(fs);
                        }

                        Console.Write('*');
                    }
                    Console.WriteLine("] ok!");
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("[XXXXXXXXXX] fail: texture not found!");
                }
                catch (Exception e)
                {
                    Console.WriteLine("[XXXXXXXXXX] fail: {0}!", e.Message);
                }

                if (spriteDir.Exists && !spriteDir.EnumerateFiles().Any())
                {
                    spriteDir.Delete();
                }
            }
        }
    }
}
