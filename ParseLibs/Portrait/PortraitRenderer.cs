﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using log4net;
using Parsers.Portrait;

namespace Parsers.Portrait {
	public class PortraitRenderer {

		private static readonly ILog logger = LogManager.GetLogger(typeof(PortraitRenderer).Name);

		/// <summary>
		/// Draws a character portrait.
		/// </summary>
		/// <param name="portraitType">PortaitType to use for drawing.</param>
		/// <param name="dna">DNA string to use for drawing.</param>
		/// <param name="properties">Properties string to use for drawing.</param>
		/// <param name="activeMods">Mods to use when drawing.</param>
		/// <param name="user">User configuration</param>
		/// <returns>Frameless portrait drawn with the given parameters.</returns>
		public Bitmap DrawPortrait(PortraitType portraitType, Portrait portrait, List<Content> activeMods, User user, Dictionary<string, Sprite> sprites) {
			logger.Info(string.Format("Drawing Portrait {0}", portrait));

			Bitmap portraitImage = new Bitmap(176, 176);
			Graphics g = Graphics.FromImage(portraitImage);

			foreach (Layer layer in portraitType.Layers) {
				logger.Debug("Drawing Layer : " + layer);

				try {
					if (sprites.ContainsKey(layer.Name)) {
						Sprite sprite = sprites[layer.Name];

						//Check if loaded; if not, then load
						if (!sprite.IsLoaded) {
							LoadSprite(sprite, activeMods, user);
						}

						//Get DNA/Properties letter, then the index of the tile to draw
						int tileIndex = GetTileIndex(portrait, sprite.FrameCount, layer);

						DrawTile(portraitType, portrait.GetDNA(), g, sprite, layer, tileIndex);

					}
					else {
						throw new FileNotFoundException("Sprite not found:" + layer);
					}

				}
				catch (Exception e) {
					logger.Error("Could not render layer " + layer + ", caused by:\n" + e.ToString());
				}
			}

			g.Dispose();
			return portraitImage;
		}

		private int GetTileIndex(Portrait portrait, int frameCount, Layer layer) {
			// TODO Refactor with layers inside Portrait
			char letter = layer.LayerType == Layer.Type.DNA ? portrait.GetDNA()[layer.Index] : portrait.GetProperties()[layer.Index]; ;
			int tileIndex = Portrait.GetTileIndexFromLetter(letter, frameCount);
			logger.Debug(string.Format("Layer Letter: {0}, Tile Index: {1}", letter, tileIndex));
			return tileIndex;
		}

		private void LoadSprite(Sprite sprite, List<Content> activeMods, User user) {
			string filePath = sprite.TextureFilePath;

			string containerPath = null;
			foreach (Content mod in activeMods) {
				string modPath = mod.AbsolutePath;
				if (File.Exists(modPath + "/" + filePath)) {
					containerPath = modPath;
					break;
				}
			}

			if (containerPath == null) {
				if (File.Exists(user.GameDir + "/" + filePath)) {
					containerPath = user.GameDir;
				}
				else {
					throw new FileNotFoundException(string.Format("Unable to find file: {0} under mods {1}, nor vanilla {2}", filePath, activeMods, user.GameDir));
				}
			}

			logger.Debug("Loading sprite from: " + containerPath);
			sprite.Load(containerPath);
		}

		private void DrawTile(PortraitType portraitType, string dna, Graphics g, Sprite sprite, Layer layer, int tileIndex) {
			Bitmap tile;
			if (layer.IsHair) {
				int hairIndex = Portrait.GetTileIndexFromLetter(dna[portraitType.HairColourIndex], portraitType.HairColours.Count);
				tile = DrawHair(sprite.Tiles[tileIndex], portraitType.HairColours[hairIndex]);

			}
			else if (layer.IsEye) {
				int eyeIndex = Portrait.GetTileIndexFromLetter(dna[portraitType.EyeColourIndex], portraitType.EyeColours.Count);
				tile = DrawEye(sprite.Tiles[tileIndex], portraitType.EyeColours[eyeIndex]);

			}
			else {
				tile = sprite.Tiles[tileIndex];
			}

			g.DrawImage(tile, 12 + layer.Offset.X, 12 + 152 - tile.Size.Height - layer.Offset.Y);
		}

		private Bitmap DrawEye(Bitmap source, Colour eyeColour) {
			Bitmap output = new Bitmap(152, 152);
			Colour colour1 = new Colour(), colour2 = new Colour();

			BitmapData bdata = source.LockBits(new Rectangle(0, 0, 152, 152), ImageLockMode.ReadOnly, source.PixelFormat);
			BitmapData odata = output.LockBits(new Rectangle(0, 0, 152, 152), ImageLockMode.ReadOnly, output.PixelFormat);
			int pixelSize = 4;

			unsafe
			{
				for (int y = 0; y < 152; y++) {
					byte* brow = (byte*)bdata.Scan0 + (y * bdata.Stride);
					byte* orow = (byte*)odata.Scan0 + (y * odata.Stride);

					for (int x = 0; x < 152; x++) {
						colour1.Red = brow[x * pixelSize + 2];
						colour1.Alpha = brow[x * pixelSize + 3];

						if (colour1.Alpha > 0) {
							colour2.Red = (byte)(255 * ((eyeColour.Red / 255f) * (colour1.Red / 255f)));
							colour2.Green = (byte)(255 * ((eyeColour.Green / 255f) * (colour1.Red / 255f)));
							colour2.Blue = (byte)(255 * ((eyeColour.Blue / 255f) * (colour1.Red / 255f)));

							orow[x * pixelSize] = colour2.Blue;
							orow[x * pixelSize + 1] = colour2.Green;
							orow[x * pixelSize + 2] = colour2.Red;
							orow[x * pixelSize + 3] = colour1.Alpha;
						}
					}
				}
			}

			source.UnlockBits(bdata);
			output.UnlockBits(odata);

			return output;
		}

		private Bitmap DrawHair(Bitmap source, Hair hairColor) {
			Bitmap output = new Bitmap(152, 152);
			Colour colour1 = new Colour(), colour2;

			BitmapData bdata = source.LockBits(new Rectangle(0, 0, 152, 152), ImageLockMode.ReadOnly, source.PixelFormat);
			BitmapData odata = output.LockBits(new Rectangle(0, 0, 152, 152), ImageLockMode.ReadOnly, output.PixelFormat);
			int pixelSize = 4;

			unsafe
			{
				for (int y = 0; y < 152; y++) {
					byte* brow = (byte*)bdata.Scan0 + (y * bdata.Stride);
					byte* orow = (byte*)odata.Scan0 + (y * odata.Stride);

					for (int x = 0; x < 152; x++) {
						colour1.Green = brow[x * pixelSize + 1];
						colour1.Alpha = brow[x * pixelSize + 3];

						if (colour1.Alpha > 0) {
							colour2 = Colour.Lerp(hairColor.Dark, hairColor.Base, Colour.Clamp(colour1.Green * 2));
							colour2 = Colour.Lerp(colour2, hairColor.Highlight, Colour.Clamp((colour1.Green - 128) * 2));

							orow[x * pixelSize] = colour2.Blue;
							orow[x * pixelSize + 1] = colour2.Green;
							orow[x * pixelSize + 2] = colour2.Red;
							orow[x * pixelSize + 3] = colour1.Alpha;
						}
					}
				}
			}

			source.UnlockBits(bdata);
			output.UnlockBits(odata);

			return output;
		}
	}
}
