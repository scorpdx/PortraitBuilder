using System;
using System.Collections.Generic;
using System.Text;

namespace PortraitBuilder.Model.Portrait
{
    public static class DefaultCharacteristics
    {
        public static Characteristic BACKGROUND = new Characteristic("Background", 0, CharacteristicType.Property, true);
        public static Characteristic HAIR = new Characteristic("Hair", 1, CharacteristicType.Property, true);
        public static Characteristic HEAD = new Characteristic("Head", 2, CharacteristicType.Property, true);
        public static Characteristic CLOTHES = new Characteristic("Clothes", 3, CharacteristicType.Property, true);
        public static Characteristic BEARD = new Characteristic("Beard", 4, CharacteristicType.Property, true);
        public static Characteristic HEADGEAR = new Characteristic("Headgear", 5, CharacteristicType.Property, true);
        public static Characteristic IMPRISONED = new Characteristic("Imprisoned", 6, CharacteristicType.Property, false);
        public static Characteristic SCARS = new Characteristic("Scars", 7, CharacteristicType.Property, false);
        public static Characteristic RED_DOTS = new Characteristic("Reddots", 8, CharacteristicType.Property, false);
        public static Characteristic BOILS = new Characteristic("Boils", 9, CharacteristicType.Property, false);
        public static Characteristic BLINDED = new Characteristic("Blinded", 10, CharacteristicType.Property, false);
        public static Characteristic PLAYER = new Characteristic("Player", 11, CharacteristicType.Property, false);
        public static Characteristic MASK = new Characteristic("Mask", 12, CharacteristicType.Property, false);
        public static Characteristic EYEPATCH = new Characteristic("Eyepatch", 13, CharacteristicType.Property, false);
        public static Characteristic MAKEUP = new Characteristic("Makeup", 14, CharacteristicType.Property, false);
        public static Characteristic MAKEUP_2 = new Characteristic("Makeup2", 15, CharacteristicType.Property, false);
        public static Characteristic JEWELRY = new Characteristic("Jewelry", 16, CharacteristicType.Property, false);
        public static Characteristic IMMORTALITY = new Characteristic("Immortality", 17, CharacteristicType.Property, false);
        public static Characteristic SPECIAL_CROWN_BEHIND = new Characteristic("Crown behind", 18, CharacteristicType.Property, false);
        public static Characteristic SPECIAL_CROWN = new Characteristic("Crown", 19, CharacteristicType.Property, false);
        public static Characteristic FRECKLES = new Characteristic("Freckles", 20, CharacteristicType.Property, false);
        public static Characteristic PHYSIQUE = new Characteristic("Physique", 21, CharacteristicType.Property, false);
        public static Characteristic PALE = new Characteristic("Pale", 22, CharacteristicType.Property, false);
        public static Characteristic BLACK_EYE = new Characteristic("Black eye", 23, CharacteristicType.Property, false);
        public static Characteristic HAIRELIP = new Characteristic("Hairelip", 24, CharacteristicType.Property, false);
        public static Characteristic SCARS_MID = new Characteristic("Scars mid", 25, CharacteristicType.Property, false);
        public static Characteristic SCARS_HIGH = new Characteristic("Scars high", 26, CharacteristicType.Property, false);
        public static Characteristic BLOOD = new Characteristic("Blood", 27, CharacteristicType.Property, false);
        public static Characteristic TATTOO = new Characteristic("Tattoo", 28, CharacteristicType.Property, false);
        public static Characteristic WARPAINT = new Characteristic("Warpaint", 29, CharacteristicType.Property, false);
        public static Characteristic POSSESSED = new Characteristic("Possessed", 30, CharacteristicType.Property, false);
        public static Characteristic OVERLAYER_BEHIND = new Characteristic("Overlayer behind", 31, CharacteristicType.Property, false);
        public static Characteristic OVERLAYER = new Characteristic("Overlayer", 32, CharacteristicType.Property, false);
        public static Characteristic UNDERMAIN = new Characteristic("Undermain", 33, CharacteristicType.Property, false);
        public static Characteristic SPECIAL_HELMET = new Characteristic("Helmet", 34, CharacteristicType.Property, false);
        public static Characteristic SPECIAL_MASK = new Characteristic("Mask", 35, CharacteristicType.Property, false);
        public static Characteristic SPECIAL_SCEPTER = new Characteristic("Scepter", 36, CharacteristicType.Property, false);
        public static Characteristic RELATIONSHIP = new Characteristic("Relationship", 37, CharacteristicType.Property, false);

        public static Characteristic NECK = new Characteristic("Neck", 0, CharacteristicType.DNA, true);
        public static Characteristic CHIN = new Characteristic("Chin", 1, CharacteristicType.DNA, true);
        public static Characteristic MOUTH = new Characteristic("Mouth", 2, CharacteristicType.DNA, true);
        public static Characteristic NOSE = new Characteristic("Nose", 3, CharacteristicType.DNA, true);
        public static Characteristic CHEEKS = new Characteristic("Cheeks", 4, CharacteristicType.DNA, true);
        public static Characteristic D5 = new Characteristic("Unused", 5, CharacteristicType.DNA, true);
        public static Characteristic EYES = new Characteristic("Eyes", 6, CharacteristicType.DNA, true);
        public static Characteristic EARS = new Characteristic("Ears", 7, CharacteristicType.DNA, true);
        public static Characteristic HAIR_COLOR = new Characteristic("Haircolor", 8, CharacteristicType.DNA, true);
        public static Characteristic EYE_COLOR = new Characteristic("Eyecolor", 9, CharacteristicType.DNA, true);
        public static Characteristic D10 = new Characteristic("Unused", 10, CharacteristicType.DNA, true);

        public static Characteristic[] DNA = new Characteristic[] { NECK, CHIN, MOUTH, NOSE, CHEEKS, D5, EYES, EARS, HAIR_COLOR, EYE_COLOR, D10 };
        public static Characteristic[] PROPERTIES = new Characteristic[] { BACKGROUND, HAIR, HEAD, CLOTHES, BEARD, HEADGEAR, IMPRISONED, SCARS, RED_DOTS, BOILS,
            BLINDED, PLAYER, MASK, EYEPATCH, MAKEUP, MAKEUP_2, JEWELRY, IMMORTALITY, SPECIAL_CROWN_BEHIND, SPECIAL_CROWN, FRECKLES, PHYSIQUE, PALE, BLACK_EYE,
            HAIRELIP, SCARS_MID, SCARS_HIGH, BLOOD, TATTOO, WARPAINT, POSSESSED, OVERLAYER_BEHIND, OVERLAYER, UNDERMAIN,
            SPECIAL_HELMET, SPECIAL_MASK, SPECIAL_SCEPTER, RELATIONSHIP };


        public static Characteristic GetProperty(int index)
        {
            if (index < PROPERTIES.Length)
            {
                return PROPERTIES[index];
            }
            else
            {
                // As defines are not parsed, always consider custom properties as valid.
                return new Characteristic("Custom", index, CharacteristicType.Property, custom: true);
            }
        }

        public static Characteristic GetDNA(int index)
        {
            try
            {
                return DNA[index];
            }
            catch (IndexOutOfRangeException)
            {
                throw new IndexOutOfRangeException($"Characteristic d{index} does not exist.");
            }
        }

    }
}
