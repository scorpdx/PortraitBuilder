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

namespace PortraitBuilder.Online
{
    public static class PortraitFunction
    {
        private static readonly Lazy<Loader> _loader = new Lazy<Loader>(() =>
        {
            var user = new User
            {
                GameDir = @"X:\Games\Steam\steamapps\common\Crusader Kings II",//readGameDir();
                ModDir = @"C:\Users\scorp\Documents\Paradox Interactive\Crusader Kings II",//readModDir(user.GameDir);
                DlcDir = "dlc/"
            };

            var loader = new Loader(user);
            loader.LoadVanilla();
            var dlcs = loader.LoadDLCs();
            loader.UpdateActiveAdditionalContent(dlcs.Where(dlc => dlc.HasPortraitData).Cast<Content>().ToList());
            loader.LoadPortraits();

            return loader;
        });

        [FunctionName("portrait")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

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

            if(!string.IsNullOrEmpty(government) && Enum.TryParse(government, true, out GovernmentType gov))
            {
                character.Government = gov;
            }

            if(!string.IsNullOrEmpty(titleRank) && Enum.TryParse(titleRank, true, out TitleRank rank))
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
            var bmp = portraitRenderer.DrawCharacter(character, loader.ActiveContents, loader.ActivePortraitData.Sprites);
            var png = SKImage.FromBitmap(bmp).Encode();

            return new FileStreamResult(png.AsStream(), "image/png");
        }
    }
}
