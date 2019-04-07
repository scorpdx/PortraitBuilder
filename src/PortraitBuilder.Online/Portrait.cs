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
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PortraitBuilder.Online
{
    public static class PortraitFunction
    {
        private static readonly Lazy<CloudBlobClient> _storageClient = new Lazy<CloudBlobClient>(() =>
        {
            string storageConnectionString = Environment.GetEnvironmentVariable("storageconnectionstring");
            var storage = CloudStorageAccount.Parse(storageConnectionString);
            return storage.CreateCloudBlobClient();
        });

        private static readonly Lazy<ValueTask<PortraitData>> _portraitPack = new Lazy<PortraitData>(() =>
        {
            var client = _storageClient.Value;
            var container = client.GetContainerReference("packs");
            var blob = container.GetBlockBlobReference("portraits.json");

            var packsPath = @"C:\Dropbox\Data\LocalRepos\scorpdx\PortraitBuilder\src\SpritePackGenerator\bin\Debug\netcoreapp3.0\packs";//Path.Combine("../", "packs/");
            var packDir = new DirectoryInfo(packsPath);

            var skiaConv = new SkiaConverter();
            var packContents = packDir.EnumerateFiles("*.json", SearchOption.AllDirectories)
                .Select(fi => fi.FullName)
                .Select(File.ReadAllText)
                .Select(json => JsonConvert.DeserializeObject<Content>(json, skiaConv));

            var loader = new PackLoader();
            foreach (var pack in packContents)
            {
                pack.AbsolutePath = Path.Combine(@"C:\Dropbox\Data\LocalRepos\scorpdx\PortraitBuilder\src\SpritePackGenerator\bin\Debug\netcoreapp3.0\", pack.AbsolutePath); //Path.Combine("../", pack.AbsolutePath);
                LoggingHelper.DefaultLogger?.LogInformation("Loaded pack {0}", pack.Name);

                loader.ActiveContent.Add(pack);
            }
            loader.LoadPortraits();
            loader.InvalidateCache();

            return loader;
        });

        [FunctionName("portrait")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            //LoggingHelper.DefaultLogger = log;

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

            var portraitBuilder = new PortraitBuilder.Engine.PortraitBuilder();
            var steps = portraitBuilder.BuildCharacter(character, loader.ActivePortraitData.Sprites)
                .Where(s => s != null)
                .ToArray();

            var sprites = steps.ToDictionary(s => s.Def.Name, s => s.Def);
            //Microsoft.WindowsAzure.Storage.Blob.blob

            var portraitRenderer = new PortraitRenderer();
            var bmp = portraitRenderer.DrawCharacter(character, loader.Cache, loader.ActivePortraitData.Sprites);
            var png = SKImage.FromBitmap(bmp).Encode();

            return new FileStreamResult(png.AsStream(), "image/png");
        }
    }
}
