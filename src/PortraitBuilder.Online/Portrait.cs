using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PortraitBuilder.Model.Portrait;
using PortraitBuilder.Engine;
using PortraitBuilder.Model;
using SkiaSharp;
using System.Linq;
using PortraitBuilder.Model.Content;
using System;
using Newtonsoft.Json;
using PortraitBuilder.ContentPacks;

namespace PortraitBuilder.Online
{
    public static class PortraitFunction
    {
        private static readonly Lazy<PackLoader> _loader = new Lazy<PackLoader>(() =>
        {
            var packsPath = @"C:\Dropbox\Data\LocalRepos\scorpdx\PortraitBuilder\src\SpritePackGenerator\bin\Debug\netcoreapp3.0\packs";
            var packDir = new System.IO.DirectoryInfo(packsPath);

            var skiaConv = new SkiaConverter();
            var packContents = packDir.EnumerateFiles("*.json", System.IO.SearchOption.AllDirectories)
                .Select(fi => fi.FullName)
                .Select(System.IO.File.ReadAllText)
                .Select(json => JsonConvert.DeserializeObject<Content>(json, skiaConv))
                .ToArray();

            foreach(var pack in packContents)
            {
                pack.AbsolutePath = System.IO.Path.Combine(@"C:\Dropbox\Data\LocalRepos\scorpdx\PortraitBuilder\src\SpritePackGenerator\bin\Debug\netcoreapp3.0\", pack.AbsolutePath);
            }

            var loader = new PackLoader();
            loader.ActiveContent.AddRange(packContents);
            loader.LoadPortraits();
            loader.InvalidateCache();

            return loader;
        });

        [FunctionName("portrait")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            LoggingHelper.DefaultLogger = log;

            string dna = req.Query["dna"];
            string properties = req.Query["properties"];
            if (string.IsNullOrEmpty(dna) || string.IsNullOrEmpty(properties))
                return new BadRequestResult();

            var ptBase = req.Query["base"];
            if (string.IsNullOrEmpty(ptBase))
                return new BadRequestResult();

            var ptClothing = req.Query["clothing"];
            var government = req.Query["government"];
            var titleRank = req.Query["titleRank"];

            var character = new Character();
            character.Import(dna, properties);

            if (!string.IsNullOrEmpty(government) && Enum.TryParse(government, true, out GovernmentType gov))
            {
                character.Government = gov;
            }

            if (!string.IsNullOrEmpty(titleRank) && Enum.TryParse(titleRank, true, out TitleRank rank))
            {
                character.Rank = rank;
            }

            var loader = _loader.Value;
            if (loader.ActivePortraitData.PortraitTypes.Count == 0)
            {
                return new StatusCodeResult(500);
            }

            if (string.IsNullOrEmpty(ptClothing))
            {
                character.PortraitType = loader.GetPortraitType($"PORTRAIT_{ptBase}");
            }
            else
            {
                character.PortraitType = loader.GetPortraitType($"PORTRAIT_{ptBase}", $"PORTRAIT_{ptClothing}");
            }

            var portraitRenderer = new PortraitRenderer();
            var bmp = portraitRenderer.DrawCharacter(character, loader.Cache, loader.ActivePortraitData.Sprites);
            var png = SKImage.FromBitmap(bmp).Encode();

            return new FileStreamResult(png.AsStream(), "image/png");
        }
    }
}
