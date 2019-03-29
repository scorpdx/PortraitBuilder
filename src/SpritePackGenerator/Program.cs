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
                var cache = new SpriteCache(Enumerable.Repeat(dlc, 1));

                foreach (var def in dlc.PortraitData.Sprites.Values)
                {
                    Console.WriteLine("Sprite {0}", def.Name);
                    using (var sprite = cache.Get(def))
                    {
                        var spritePath = Path.Combine(dlc.AbsolutePath, Path.GetDirectoryName(def.TextureFilePath), Path.GetFileNameWithoutExtension(def.TextureFilePath), def.Name + "/");
                        var spriteDir = new DirectoryInfo(spritePath).CreateSubdirectory(".");

                        try
                        {
                            var tiles = sprite.Tiles;
                            Console.Write("[{0}]", new string(' ', tiles.Count));
                            Console.Write(new string('\b', tiles.Count + 1));
                            for (int i = 0; i < tiles.Count; i++)
                            {
                                Console.Write("*");

                                var tile = tiles[i];
                                var tilePath = Path.Combine(spritePath, $"{i}.png");
                                using (var fs = File.Create(tilePath))
                                using (var pixmap = tile.PeekPixels())
                                    pixmap.Encode(SkiaSharp.SKPngEncoderOptions.Default).SaveTo(fs);
                            }
                            Console.WriteLine("] ok!");
                        }
                        catch (Exception e) when (e is FileNotFoundException)
                        {
                            Console.WriteLine("] fail: texture not found!");
                        }
                    }
                }
            }
            //loader.UpdateActiveAdditionalContent(activeDlcs);
            //loader.LoadPortraits();

            Console.WriteLine("Loaded.");
            //foreach (var layer in character.PortraitType.Layers)
            //{
            //    if (!DrawLayer(layer, canvas, character, cache, sprites))
            //    {
            //        logger.LogWarning($"Could not render layer {layer}");
            //    }
            //}

            //DrawBorder(character, canvas, cache, sprites);
        }
    }
}
