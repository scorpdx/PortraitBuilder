using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using PortraitBuilder.Model.Portrait;
using PortraitBuilder.Engine;
using PortraitBuilder.Model;
using SkiaSharp;
using System.Linq;
using System;
using Newtonsoft.Json;
using PortraitBuilder.ContentPacks;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Collections.Generic;
using System.Collections.Concurrent;

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

        private static readonly Lazy<Task<PortraitData>> _portraitPack = new Lazy<Task<PortraitData>>(async () =>
        {
            var client = _storageClient.Value;

            var container = client.GetContainerReference("packs");
            var blob = container.GetBlockBlobReference("portraits.json");

            var skiaConv = new SkiaConverter();
            var json = await blob.DownloadTextAsync();

            return JsonConvert.DeserializeObject<PortraitData>(json, skiaConv);
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

            var portrait = await _portraitPack.Value;
            if (!portrait.PortraitTypes.Any())
            {
                return new StatusCodeResult(500);
            }

            var basePortraitType = portrait.PortraitTypes[$"PORTRAIT_{ptBase}"];
            if (string.IsNullOrEmpty(ptClothing))
            {
                character.PortraitType = basePortraitType;
            }
            else
            {
                var clothingPortraitType = portrait.PortraitTypes[$"PORTRAIT_{ptClothing}"];
                character.PortraitType = basePortraitType.Merge(clothingPortraitType);
            }

            var steps = PortraitBuilder.Engine.PortraitBuilder.BuildCharacter(character, portrait.Sprites)
                .Where(s => s != null)
                .ToArray();

            var container = _storageClient.Value.GetContainerReference("packs");

            var defs = new ConcurrentDictionary<SpriteDef, HashSet<int>>();
            foreach (var step in steps)
            {
                var set = defs.GetOrAdd(step.Def, new HashSet<int>());
                set.Add(step.TileIndex);
            }

            string GetBlobPath(SpriteDef def)
                => def.TextureFilePath.StartsWith("packs/") ? def.TextureFilePath.Substring("packs/".Length) : def.TextureFilePath;

            async Task<SKBitmap> DownloadBlobAsync(string path)
            {
                var blob = container.GetBlockBlobReference(path);
                using (var stream = await blob.OpenReadAsync())
                {
                    return SKBitmap.Decode(stream);
                }
            }

            var bitmapTasks = defs
                .ToDictionary(kvp => kvp.Key, kvp =>
                {
                    var tasks = new Task<SKBitmap>[kvp.Key.FrameCount];
                    foreach (var index in kvp.Value)
                    {
                        tasks[index] = DownloadBlobAsync($"{GetBlobPath(kvp.Key)}/{index}.png");
                    }
                    return tasks;
                });

            foreach(var step in steps)
            {
                var bitmapTask = bitmapTasks[step.Def][step.TileIndex];
                if (bitmapTask == null) continue;

                step.Tile = await bitmapTask;
            }

            var bmp = PortraitRenderer.DrawPortrait(steps);
            var png = SKImage.FromBitmap(bmp).Encode();

            return new FileStreamResult(png.AsStream(), "image/png");
        }
    }
}
