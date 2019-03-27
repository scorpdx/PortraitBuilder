﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using Hime.Redist;
using log4net;
using PortraitBuilder.Model.Portrait;
using SkiaSharp;

namespace PortraitBuilder.Parser
{

    using static EncodingHelper;
    /// <summary>
    /// Handles the parsing of portraits *.gfx files.
    /// </summary>
    public class PortraitReader
    {

        private static readonly ILog logger = LogManager.GetLogger(typeof(PortraitReader));

        /// <summary>
        /// Stateless portrait_offsets.txt file scanner
        /// </summary>
        private PortraitOffsetReader portraitOffsetReader = new PortraitOffsetReader();

        public string BaseDir { get; }

        public string InterfaceDirectory => Path.Combine(BaseDir, "interface");

        public string PortraitsDirectory => Path.Combine(InterfaceDirectory, "portraits");

        public string PortraitOffsetsDirectory => Path.Combine(InterfaceDirectory, "portrait_offsets");

        public PortraitReader(string dir)
        {
            this.BaseDir = dir;
        }

        /// <summary>
        /// Parse Portrait data.
        /// 
        /// Parsing errors are catched at layer, propertyType or file level, so the PortraitData may be partial or even empty.
        /// </summary>
        /// <param name="dir">The content root directory to parse from</param>
        /// <returns></returns>
        public PortraitData Parse()
        {
            var data = new PortraitData();
            try
            {
                var fileNames = Enumerable.Empty<string>();
                logger.Debug("Scanning for portrait data files in " + BaseDir);
                logger.Debug("Directories: " + Directory.GetDirectories(BaseDir));

                if (Directory.Exists(InterfaceDirectory))
                {
                    fileNames = fileNames.Concat(Directory.EnumerateFiles(InterfaceDirectory, "*.gfx"));
                }
                else
                {
                    logger.Debug("Folder not found: " + Path.Combine(BaseDir, "interface"));
                }

                // interface/portraits seems to be loaded after interface/, and override (cf byzantinegfx)
                if (Directory.Exists(PortraitsDirectory))
                {
                    fileNames = fileNames.Concat(Directory.EnumerateFiles(PortraitsDirectory, "*.gfx"));
                }
                else
                {
                    logger.Debug("Folder not found: " + Path.Combine(BaseDir, "interface", "portraits"));
                }

                foreach (string fileName in fileNames)
                {
                    Parse(fileName, data);
                }

                if (Directory.Exists(PortraitOffsetsDirectory))
                {
                    data.Offsets = Directory.EnumerateFiles(PortraitOffsetsDirectory, "*.txt")
                        .SelectMany(portraitOffsetReader.Parse)
                        .Concat(data.Offsets)
                        .ToLookup(d => d.Key, d => d.Value)
                        .ToDictionary(d => d.Key, d => d.First());
                }
            }
            catch (Exception e)
            {
                logger.Error("Failed to parse portrait data in " + BaseDir, e);
            }

            return data;
        }

        private static HashSet<string> BadFiles { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "DefaultDialog.gfx",
            "EU3_mapitems.gfx",
            "chatfonts.gfx",
            "fonts.gfx",
            "mapitems.gfx"
        };

        /// <summary>
        /// Parses a given portrait.gfx file.
        /// </summary>
        /// <param name="filename">Path of the file to parse.</param>
        private void Parse(string filename, PortraitData data)
        {
            var fi = new FileInfo(filename);
            if (!fi.Exists)
            {
                logger.Error($"File not found: {filename}");
                return;
            }

            // Exclude vanilla files with known errors
            if (BadFiles.Contains(fi.Name))
            {
                logger.Info($"Skipping parsing of file: {filename}");
                return;
            }

            //Check the file isn't empty
            var hasContent = File.ReadLines(filename, WesternEncoding)
                .Select(l => l.Trim())
                .Any(l => !l.StartsWith("#"));

            if (!hasContent)
            {
                logger.Warn($"File is empty: {filename}");
                return;
            }

            ParseResult result;
            using (var fs = File.OpenRead(filename))
            using (var fileReader = new StreamReader(fs, WesternEncoding))
            {
                //Parse the file
                PortraitReaderLexer lexer = new PortraitReaderLexer(fileReader);
                PortraitReaderParser parser = new PortraitReaderParser(lexer);

                result = parser.Parse();
            }

            if (!result.IsSuccess)
            {
                logger.Error($"Lexical error in file {fi.Name}, line {result.Errors}");
                return;
            }

            ParseTree(result.Root, filename, data);
        }

        private void ParseTree(ASTNode root, string filename, PortraitData data)
        {
            foreach (ASTNode child in root.Children)
            {
                ParsePortraits(child, filename, data);
            }
        }

        private void ParsePortraits(ASTNode node, string filename, PortraitData data)
        {
            var children = node.Children.Where(child => child.Symbol.Name == "groupOption");
            foreach (ASTNode child in children)
            {
                var id = child.Children[0].Value;
                try
                {
                    switch (id)
                    {
                        case "spriteType":
                            var sprite = ParseSpriteType(child, filename);
                            if (data.Sprites.ContainsKey(sprite.Name))
                            {
                                logger.Debug($"Sprite {sprite.Name} already exists. Replacing.");
                                data.Sprites.Remove(sprite.Name);
                            }
                            data.Sprites.Add(sprite.Name, sprite);
                            break;
                        case "portraitType":
                            var portraitType = ParsePortraitType(child, filename);
                            if (data.PortraitTypes.ContainsKey(portraitType.Name))
                            {
                                logger.Debug($"Portrait type {portraitType.Name} already exists. Replacing.");
                                data.PortraitTypes.Remove(portraitType.Name);
                            }
                            data.PortraitTypes.Add(portraitType.Name, portraitType);
                            break;
                    }
                }
                catch (Exception e)
                {
                    logger.Error($"Could not parse {id} in file {filename}", e);
                }
            }
        }

        private PortraitType ParsePortraitType(ASTNode node, string filename)
        {
            PortraitType portraitType = new PortraitType();
            portraitType.Filename = filename;

            List<ASTNode> children = node.Children.Where(child => child.Symbol.Name == "Option").ToList();
            string id, value;
            ASTNode token;
            foreach (ASTNode child in children)
            {
                token = child.Children[0];

                if (token.Children.Count > 1 == false)
                    continue;

                id = token.Children[0].Value;
                value = token.Children[1].Value;

                switch (token.Symbol.Name)
                {
                    case "stringOption":
                        if (id == "name")
                            portraitType.Name = value.Replace("\"", "");
                        if (id == "effectFile")
                            portraitType.EffectFile = value.Replace("\"", "").Replace(@"\\", @"\");
                        break;
                    case "numberOption":
                        if (id == "hair_color_index")
                            portraitType.HairColourIndex = parseInt(value);
                        if (id == "eye_color_index")
                            portraitType.EyeColourIndex = parseInt(value);
                        break;
                }
            }

            logger.Debug("Type parsed: ");
            logger.Debug(" --ID: " + portraitType.Name);
            logger.Debug(" --Hair Colour Index: " + portraitType.HairColourIndex);
            logger.Debug(" --Eye Colour Index: " + portraitType.EyeColourIndex);

            // layer = {}
            portraitType.Layers.AddRange(ParseLayers(node.Children.Single(c => c.Symbol.Name == "layerGroup"), filename));

            // headgear_that_hides_hair = {}
            children = node.Children.Where(c => c.Symbol.Name == "cultureGroup").ToList();
            if (children.Count > 0)
            {
                foreach (ASTNode child in children[0].Children)
                    portraitType.HeadgearThatHidesHair.Add(parseInt(child.Value));
            }

            // hair_color = {} / eye_color = {}
            children = node.Children.Where(c => c.Symbol.Name == "groupOption").ToList();

            foreach (ASTNode child in children)
            {
                id = child.Children[0].Value;

                if (id == "hair_color")
                {
                    portraitType.HairColours.AddRange(ParseHairColours(child));
                }
                else if (id == "eye_color")
                {
                    portraitType.EyeColours.AddRange(ParseEyeColours(child));
                }
            }

            return portraitType;
        }

        private List<SKColor> ParseEyeColours(ASTNode node)
        {
            List<SKColor> colours = new List<SKColor>();
            IEnumerable<ASTNode> children = node.Children.Where(child => child.Symbol.Name == "colourGroup");

            foreach (ASTNode child in children)
            {
                colours.Add(ParseColor(child));
            }
            return colours;
        }

        private List<Hair> ParseHairColours(ASTNode node)
        {
            List<Hair> hairs = new List<Hair>();
            List<ASTNode> children = node.Children.Where(child => child.Symbol.Name == "colourGroup").ToList();

            for (int i = 0; i < children.Count; i += 3)
            {
                logger.Debug(" --Parsing Hair colours");

                var h_dark = ParseColor(children[i]);
                logger.Debug("   --Dark: " + h_dark);

                var h_base = ParseColor(children[i + 1]);
                logger.Debug("   --Base: " + h_base);

                var h_highlight = ParseColor(children[i + 2]);
                logger.Debug("   --Highlight: " + h_highlight);

                hairs.Add(new Hair(h_dark, h_base, h_highlight));
            }
            return hairs;
        }

        private SKColor ParseColor(ASTNode child)
        {
            if (!byte.TryParse(child.Children[0].Value, out byte red)
                || !byte.TryParse(child.Children[1].Value, out byte green)
                || !byte.TryParse(child.Children[2].Value, out byte blue))
                throw new InvalidOperationException($"Failed to parse color {child}");

            var color = new SKColor(red, green, blue);
            logger.Debug(" --Colour Parsed: " + color);
            return color;
        }

        private List<Layer> ParseLayers(ASTNode node, string filename)
        {
            List<Layer> layers = new List<Layer>();
            foreach (ASTNode child in node.Children)
            {
                try
                {
                    layers.Add(ParseLayer(child, filename));
                }
                catch (Exception e)
                {
                    logger.Error(string.Format("Could not parse layer {0} in file {1}", child.Value, filename), e);
                }
            }
            return layers;
        }

        private Layer ParseLayer(ASTNode node, string filename)
        {
            string[] layerParts = node.Value.Replace("\"", "").Split(':');

            Layer layer = new Layer();
            layer.Filename = filename;
            layer.Name = layerParts[0];

            for (int i = 1; i < layerParts.Length; i++)
            {
                if (layerParts[i].StartsWith("d"))
                {
                    layer.Characteristic = Characteristic.getDna(parseInt(layerParts[i].Substring(1)));
                }
                else if (layerParts[i].StartsWith("p"))
                {
                    layer.Characteristic = Characteristic.getProperty(parseInt(layerParts[i].Substring(1)));
                }
                else if (layerParts[i] == "h" || layerParts[i] == "x")
                {
                    layer.IsHair = true;
                }
                else if (layerParts[i] == "e")
                {
                    layer.IsEye = true;
                }
                else if (layerParts[i] == "y")
                {
                    layer.DontRefreshIfValid = true;
                }
                else if (layerParts[i].StartsWith("o"))
                {
                    string[] offsets = layerParts[i].Substring(1).Split('x');
                    layer.Offset = new Point(parseInt(offsets[0]), parseInt(offsets[1]));
                }
                else if (layerParts[i].StartsWith("c"))
                {
                    layer.CultureIndex = parseInt(layerParts[i].Substring(1));
                }
                else
                {
                    logger.Warn(string.Format("Unkown syntax \"{0}\", for layer {1} in file {2}", layerParts[i], layer, filename));
                }
            }

            if (layer.Characteristic == null && layer.CultureIndex == -1)
            {
                logger.Error(string.Format("Missing characterstic for layer {0} in file {1}", layer, filename));
            }

            logger.Debug(" --Layer Parsed: " + layer);

            return layer;
        }

        private Sprite ParseSpriteType(ASTNode node, string filename)
        {
            Sprite sprite = new Sprite(filename);

            IEnumerable<ASTNode> children = node.Children.Where(child => child.Symbol.Name == "Option");
            string id, value;
            ASTNode token;
            foreach (ASTNode child in children)
            {
                token = child.Children[0];

                if (token.Children.Count > 1 == false)
                    continue;

                id = token.Children[0].Value;
                value = token.Children[1].Value;

                switch (token.Symbol.Name)
                {
                    case "stringOption":
                    case "idOption": // Case of unquoted key/value
                        if (id == "name")
                            sprite.Name = value.Replace("\"", "");
                        if (id == "texturefile" || id == "textureFile")
                            sprite.TextureFilePath = value.Replace("\"", "").Replace(@"\\", @"\");
                        break;
                    case "boolOption":
                        if (id == "norefcount")
                            sprite.NoRefCount = value == "yes";
                        break;
                    case "numberOption":
                        if (id == "noOfFrames" || id == "noOfframes")
                            sprite.FrameCount = parseInt(value);
                        break;
                }
            }
            logger.Debug("Sprite Parsed: " + sprite);

            return sprite;
        }

        /// <summary>
        /// int.Parse does not put the parsed text in the FormatException !
        /// </summary>
        private int parseInt(string s)
        {
            try
            {
                return int.Parse(s);
            }
            catch (FormatException e)
            {
                throw new FormatException("The string " + s + " cannot be parsed as an int", e);
            }
        }

        private byte parseByte(string s)
        {
            try
            {
                return byte.Parse(s);
            }
            catch (OverflowException e)
            {
                throw new OverflowException("The string " + s + " cannot be parsed as a byte", e);
            }
        }
    }
}
