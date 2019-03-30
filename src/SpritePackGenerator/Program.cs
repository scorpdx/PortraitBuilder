using PortraitBuilder.Engine;
using PortraitBuilder.Model;
using PortraitBuilder.Model.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

            var loader = new Loader();
            Console.WriteLine("Loading vanilla content from {0}", user.GameDir);
            loader.LoadVanilla(user.GameDir);
            ExtractContent(loader.Vanilla, false);
            ExtractContentSprites(loader.Vanilla, false);

            Console.WriteLine("Loading DLC content from {0}", user.ModDir);
            Console.WriteLine("Saving to {0}", user.DlcDir);
            var activeDlcs = new List<Content>();
            foreach (Content dlc in loader.LoadDLCs(user.GameDir, user.DlcDir))
            {
                ExtractContent(dlc, true);
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
            }

            Console.WriteLine("Loaded.");
        }

        private static void ExtractContent(Content content, bool dlc)
        {
            var definitionPath = Path.Combine("packs", dlc ? content.AbsolutePath : "vanilla", Path.ChangeExtension(content.Name, ".json"));
            var definitionFile = new FileInfo(definitionPath);
            definitionFile.Directory.Create();

            if (definitionFile.Exists)
                return;

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(content);
            File.WriteAllText(definitionPath, json);
        }

        private static void ExtractContentSprites(Content content, bool dlc)
        {
            using var cache = new SpriteCache(Enumerable.Repeat(content, 1));
            foreach (var def in content.PortraitData.Sprites.Values)
            {
                Console.WriteLine("Sprite {0}", def.Name);
                using (var sprite = cache.Get(def))
                {
                    var spritePath = Path.Combine("packs", dlc ? content.AbsolutePath : "vanilla", "tiles/", def.Name + "/");
                    var spriteDir = new DirectoryInfo(spritePath).CreateSubdirectory(".");

                    try
                    {
                        var tiles = sprite.Tiles;
                        Console.Write("[{0}]", new string(' ', tiles.Count));
                        Console.Write(new string('\b', tiles.Count + 1));
                        for (int i = 0; i < tiles.Count; i++)
                        {
                            var tile = tiles[i];
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
                    catch (FileNotFoundException e)
                    {
                        Console.WriteLine("[XXXXXXXXXX] fail: texture not found!");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("[XXXXXXXXXX] fail: {0}!", e.Message);
                    }
                }
            }
        }
    }
}
