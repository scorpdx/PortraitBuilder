using SkiaSharp;
using System.Collections.Generic;
using System.Linq;

namespace PortraitBuilder.Model.Portrait
{

    /// <summary>
    /// Definition of how a portrait for a given ethnicity, sex, age and date is composed.
    /// </summary>
    public class PortraitType
    {

        /// <summary>
        /// E.g. PORTRAIT_westerngfx_male
        /// </summary>
        public string Name { get; set; }

        public string EffectFile { get; set; }

        /// <summary>
        /// Letter index of the hair colour in the DNA string
        /// </summary>
        public int HairColourIndex { get; set; } = DefaultCharacteristics.HAIR_COLOR.Index;

        /// <summary>
        /// Letter index of the eye colour in the DNA string
        /// </summary>
        public int EyeColourIndex { get; set; } = DefaultCharacteristics.EYE_COLOR.Index;

        /// <summary>
        /// List of layers composing the portraitType definition
        /// </summary>
        public List<Layer> Layers { get; set; } = new List<Layer>();

        public List<Hair> HairColours { get; set; } = new List<Hair>();
        public List<SKColor> EyeColours { get; set; } = new List<SKColor>();

        /// <summary>
        /// TODO How is this used in-game ?
        /// </summary>
        public List<int> HeadgearThatHidesHair { get; set; } = new List<int>();

        public IEnumerable<Characteristic> CustomCharacteristics
            => Layers.Select(l => l.Characteristic).Where(c => c?.Custom ?? false);

        public override string ToString()
            => $"Name: {Name}, Layers: {Layers.Count}, HairColours: {HairColours.Count}, EyeColours: {EyeColours.Count}";

        /// <summary>
        /// Whether this portraitType is a base portraitType. 
        /// 
        /// Base portraitTypes can also be used as override portraitTypes, with culture-indexed layers replacing the ones of another base.
        /// </summary>
        public bool IsBasePortraitType()
            => HairColours.Any();

        public Layer GetCultureLayer(int cultureIndex)
            => Layers.FirstOrDefault(l => l.CultureIndex == cultureIndex);
    }
}