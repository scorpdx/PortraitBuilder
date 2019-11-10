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
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Buffers;
using static PortraitBuilder.Online.BlobLoader;

namespace PortraitBuilder.Online
{
    public static class PortraitFunction
    {
        [FunctionName("character")]
        public static async Task<IActionResult> BuildCharacter(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)]PortraitBuilder.Online.Models.Character characterModel)
        {
            var character = new Character();
            character.Import(characterModel.DNA, characterModel.Properties);
            if (Enum.TryParse(characterModel.Government, ignoreCase: true, out GovernmentType government))
            {
                character.Government = government;
            }
            if (Enum.TryParse(characterModel.Rank, ignoreCase: true, out TitleRank rank))
            {
                character.Rank = rank;
            }

            var portrait = await _portraitPack.Value;
            if (!portrait.PortraitTypes.Any())
            {
                return new StatusCodeResult(500);
            }

            var cultureLookup = await _cultureLookup.Value;
            if (!cultureLookup.Any())
            {
                return new StatusCodeResult(500);
            }

            string sexSuffix = characterModel.Female.GetValueOrDefault() ? "female" : "male";
            string periodSuffix = default;
            if (characterModel.Year.HasValue)
            {
                var year = characterModel.Year.Value;
                periodSuffix = year < 950 ? "early" : year > 1250 ? "late" : "";
            }

            string ageSuffix = default;
            if (characterModel.Age.HasValue)
            {
                var age = characterModel.Age.Value;
                //Confusingly, children's portraits are defined
                //with the age suffix before the sex suffix
                ageSuffix = age <= 30 ? "" : age < 50 ? "1" : "2";
                character.IsChild = age < 16;
            }

            var religiousLookup = await _religiousClothingLookup.Value;
            if (characterModel.Religion != null && religiousLookup.TryGetValue(characterModel.Religion, out (int, int) clothingInfo))
            {
                var religiousClothingHead = clothingInfo.Item1;
                var religiousClothingPriest = clothingInfo.Item2;

                if (characterModel.ReligiousHead.GetValueOrDefault())
                {
                    character.ReligiousClothingOverride = religiousClothingHead;
                }
                else if (character.Government == GovernmentType.Theocracy)
                {
                    character.ReligiousClothingOverride = religiousClothingPriest;
                }
            }

            PortraitType portraitType = default;
            var fallbacks = new List<PortraitType>();
            foreach (var graphicalCulture in cultureLookup[characterModel.Culture])
            {
                bool omitPeriod = false;
                bool omitAge = false;
                PortraitType foundPortraitType;

                while (!portrait.PortraitTypes.TryGetValue(
                    $"PORTRAIT_{graphicalCulture}_{sexSuffix}{(omitPeriod ? "" : periodSuffix)}{(omitAge ? "" : ageSuffix)}",
                    out foundPortraitType))
                {
                    if (omitPeriod && omitAge)
                        break;
                    else if (!omitPeriod)
                        omitPeriod = true;
                    else if (!omitAge)
                        omitAge = true;
                }

                if (foundPortraitType == null)
                    continue;

                portraitType ??= foundPortraitType;
                fallbacks.Add(foundPortraitType);
            }

            if (portraitType == null)
            {
                return new StatusCodeResult(500);
            }

            character.PortraitType = portraitType;
            character.FallbackPortraitTypes = fallbacks;

            using var portraitBitmap = await DrawPortraitInternal(character);
            using var portraitPixmap = portraitBitmap.PeekPixels();

            var portraitPng = portraitPixmap.Encode(SKPngEncoderOptions.Default);
            return new FileStreamResult(portraitPng.AsStream(), "image/png");
        }

        [FunctionName("portrait")]
        public static async Task<IActionResult> DrawPortrait(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
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

            if (!portrait.PortraitTypes.TryGetValue($"PORTRAIT_{ptBase}", out PortraitType basePortraitType))
            {
                return new StatusCodeResult(500);
            }

            if (string.IsNullOrEmpty(ptClothing))
            {
                character.PortraitType = basePortraitType;
            }
            else
            {
                var clothingPortraitType = portrait.PortraitTypes[$"PORTRAIT_{ptClothing}"];
                character.PortraitType = basePortraitType.Merge(clothingPortraitType);
            }

            using var portraitBitmap = await DrawPortraitInternal(character);
            using var portraitPixmap = portraitBitmap.PeekPixels();

            var portraitPng = portraitPixmap.Encode(SKPngEncoderOptions.Default);
            return new FileStreamResult(portraitPng.AsStream(), "image/png");
        }

        private static async Task<SKBitmap> DrawPortraitInternal(Character character)
        {
            var portrait = await _portraitPack.Value;
            if (!portrait.PortraitTypes.Any())
            {
                throw new InvalidOperationException("no PortraitTypes found in loaded portrait pack");
            }

            var buildSteps = character.IsChild
                ? PortraitBuilder.Engine.PortraitBuilder.BuildChild(character, portrait.Sprites)
                : PortraitBuilder.Engine.PortraitBuilder.BuildCharacter(character, portrait.Sprites);

            var steps = buildSteps
                .Where(s => s != null)
                .ToArray();

            var container = _storageClient.Value.GetContainerReference("packs");

            var defs = new ConcurrentDictionary<SpriteDef, HashSet<int>>();
            foreach (var step in steps)
            {
                var set = defs.GetOrAdd(step.Def, new HashSet<int>());
                set.Add(step.TileIndex);
            }

            static string GetBlobPath(SpriteDef def)
                => def.TextureFilePath.StartsWith("packs/") ? def.TextureFilePath.Substring("packs/".Length) : def.TextureFilePath;

            async Task<SKBitmap> DownloadBitmapAsync(string path)
            {
                var blob = container.GetBlockBlobReference(path);
                using var stream = await blob.OpenReadAsync();

                var bmp = SKBitmap.Decode(stream);
                bmp.SetImmutable();
                return bmp;
            }

            var bitmapTasks = defs
                .ToDictionary(kvp => kvp.Key, kvp =>
                {
                    var tasks = ArrayPool<Task<SKBitmap>>.Shared.Rent(kvp.Key.FrameCount);
                    foreach (var index in kvp.Value)
                    {
                        tasks[index] = _blobTileCache.GetOrAdd($"{GetBlobPath(kvp.Key)}/{index}.png", key => DownloadBitmapAsync(key));
                    }
                    return tasks;
                });

            foreach (var step in steps)
            {
                var bitmapTask = bitmapTasks[step.Def][step.TileIndex];
                if (bitmapTask == null) continue;

                step.Tile = await bitmapTask;
            }

            var portraitBitmap = PortraitRenderer.DrawPortrait(steps);
            foreach (var array in bitmapTasks.Values)
            {
                ArrayPool<Task<SKBitmap>>.Shared.Return(array);
            }
            return portraitBitmap;
        }

        [FunctionName("portraittypes")]
        public static async Task<IActionResult> GetPortraitTypes(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req)
        {
            var portrait = await _portraitPack.Value;
            if (!portrait.PortraitTypes.Any())
            {
                return new StatusCodeResult(500);
            }

            return new JsonResult(portrait.PortraitTypes);
        }
    }
}
