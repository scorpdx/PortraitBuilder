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

namespace PortraitBuilder.Online
{
    public static class PortraitFunction
    {
        [FunctionName("portrait")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string dna = req.Query["dna"];
            string properties = req.Query["properties"];
            if (string.IsNullOrEmpty(dna) || string.IsNullOrEmpty(properties))
                return new BadRequestResult();

            string customProperties = req.Query["customProperties"];

            var character = new Character();
            character.Import(dna, properties + customProperties);

            // Reflect on dropdown
            //updateSelectedCharacteristicValues(portrait);

            //started = true;

            var portraitRenderer = new PortraitRenderer();
            User user = new User
            {
                GameDir = @"X:\Games\Steam\steamapps\common\Crusader Kings II",//readGameDir();
                ModDir = @"C:\Users\scorp\Documents\Paradox Interactive\Crusader Kings II",//readModDir(user.GameDir);
                DlcDir = "dlc/"
            };
            var loader = new Loader(user);
            loader.LoadVanilla();
            loader.LoadPortraits();
            if (loader.ActivePortraitData.PortraitTypes.Count == 0)
            {
                //logger.LogCritical("No portrait types found.");
                //return;
            }
            var selectedPortraitType = "andalusiangfx_male";
            character.PortraitType = loader.GetPortraitType($"PORTRAIT_{selectedPortraitType}");

            var bmp = portraitRenderer.DrawCharacter(character, loader.ActiveContents, loader.ActivePortraitData.Sprites);
            var png = SKImage.FromBitmap(bmp).Encode();

            return new FileStreamResult(png.AsStream(), "image/png");
        }
    }
}
