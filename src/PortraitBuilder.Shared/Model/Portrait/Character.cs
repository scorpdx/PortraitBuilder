using System;

namespace PortraitBuilder.Model.Portrait
{

    /// <summary>
    /// Input data (dna and properties) used to display a portrait.
    /// </summary>
    public class Character
    {
        public string DNA { get; private set; } = "aaaaa0aaaa0";

        public string Properties { get; private set; } = "aa0aaaaaaaa000";

        public PortraitType PortraitType { get; set; }

        /// <summary>
        /// Index of rank in border sprite
        /// </summary>
        public TitleRank Rank { get; set; }

        /// <summary>
        /// Index of government in PortraitRenderer#governmentSpriteSuffix.
        /// Hardcoded to vanilla governments only.
        /// </summary>
        public GovernmentType Government { get; set; }

        public Sex Sex
        {
            get
            {
                if (PortraitType.Name.Contains("female"))
                {
                    return Sex.Female;
                }
                else if (PortraitType.Name.Contains("male"))
                {
                    return Sex.Male;
                }
                else throw new InvalidOperationException("Unknown sex");//return default;
            }
        }

        public void Import(string dna, string properties)
        {
            if (string.IsNullOrEmpty(dna))
                throw new ArgumentNullException(nameof(dna));
            if (string.IsNullOrEmpty(properties))
                throw new ArgumentNullException(nameof(properties));

            void ThrowForLength(string paramName) => throw new ArgumentException($"{paramName} is too short");

            if (dna.Length < 10)
                ThrowForLength(nameof(dna));
            if (properties.Length < 11)
                ThrowForLength(nameof(properties));

            this.DNA = dna;
            this.Properties = properties;
        }

        public bool TryGetLetter(Characteristic characteristic, out char letter)
        {
            if (characteristic == DefaultCharacteristics.HAIR_COLOR)
            {
                letter = DNA[PortraitType.HairColourIndex];
            }
            else if (characteristic == DefaultCharacteristics.EYE_COLOR)
            {
                letter = DNA[PortraitType.EyeColourIndex];
            }
            else if (characteristic.Type == Characteristic.CharacteristicType.DNA)
            {
                letter = DNA[characteristic.Index];
            }
            else if (characteristic.Index < Properties.Length)
            {
                letter = Properties[characteristic.Index];
            }
            else
            {
                letter = default;
                return false;
            }

            return true;
        }


        public override string ToString() => $"DNA: {DNA}, Properties: {Properties}";

        /// <summary>
        /// Converts a letter to an index
        /// 
        /// In case "letter" is 0, the index is 0
        /// </summary>
        /// <param name="letter"></param>
        /// <param name="total"></param>
        /// <returns></returns>
        public static int GetIndex(char letter, int total)
        {
            if (total == 0)
                return -1;

            if (letter == '0')
                return 0;

            int index = (Array.IndexOf(Alphabet, letter) + 1) % total;
            if (index == total)
            {
                index = 0;
            }
            return index;
        }

        /// <summary>
        /// Converts an index to a letter
        /// 
        /// In case index is 0, the "letter" is 0, rather than Letters[(total - 1)%26]
        /// This is to match how vanilla deals with characteristics in character history
        /// 
        /// Note: in some cases total maximulm 27 (blgrabs mod)
        /// </summary>
        public static char GetLetter(int index) => index == -1 || index == 0 ? '0' : Alphabet[index - 1];

        private static char[] Alphabet { get; } = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
    }
}
