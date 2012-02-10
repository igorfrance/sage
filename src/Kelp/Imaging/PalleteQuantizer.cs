/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
namespace Kelp.Imaging
{
	using System;
	using System.Collections;
	using System.Drawing;
	using System.Drawing.Imaging;

	/// <summary>
	/// Provides a pelette based quantizer that can be used when converting images to 8-bit.
	/// </summary>
	public unsafe class PaletteQuantizer : Quantizer
	{
		private readonly Hashtable colorMap;
		private readonly Color[] colors;

		/// <summary>
		/// Initializes a new instance of the <see cref="PaletteQuantizer"/> class.
		/// </summary>
		/// <param name="image">The image that contains the colors to use.</param>
		public PaletteQuantizer(Bitmap image)
			: base(true)
		{
			ArrayList palette = GetImageColors(image);
			colorMap = new Hashtable();
			colors = new Color[palette.Count];
			palette.CopyTo(colors);
		}

		/// <summary>
		/// Creates a new ArrayList containing first up to 255 colors from the image.
		/// </summary>
		/// <param name="image">The source image</param>
		/// <returns>The array of colors</returns>
		public static ArrayList GetImageColors(Bitmap image)
		{
			ArrayList palette = new ArrayList();

			for (int x = 1; x < image.Width; x++)
			{
				if (palette.Count == 255)
					break;

				for (int y = 1; y < image.Height; y++)
				{
					Color c = image.GetPixel(x, y);
					if (!palette.Contains(c))
						palette.Add(c);

					if (palette.Count == 255)
						break;
				}
			}

			return palette;
		}

		/// <summary>
		/// Override this to process the pixel in the second pass of the algorithm
		/// </summary>
		/// <param name="pixel">The pixel to quantize</param>
		/// <returns>The quantized value</returns>
		protected override byte QuantizePixel(Color32* pixel)
		{
			byte colorIndex = 0;
			int colorHash = pixel->ARGB;

			if (colorMap.ContainsKey(colorHash))
				colorIndex = (byte) colorMap[colorHash];
			else
			{
				// Not found - loop through the palette and find the nearest match.
				// Firstly check the alpha value - if 0, lookup the transparent color
				if (0 == pixel->Alpha)
				{
					// Transparent. Lookup the first color with an alpha value of 0
					for (int index = 0; index < colors.Length; index++)
					{
						if (0 == colors[index].A)
						{
							colorIndex = (byte) index;
							break;
						}
					}
				}
				else
				{
					// Not transparent...
					int leastDistance = int.MaxValue;
					int red = pixel->Red;
					int green = pixel->Green;
					int blue = pixel->Blue;

					// Loop through the entire palette, looking for the closest color match
					for (int index = 0; index < colors.Length; index++)
					{
						Color paletteColor = colors[index];

						int redDistance = paletteColor.R - red;
						int greenDistance = paletteColor.G - green;
						int blueDistance = paletteColor.B - blue;

						int distance = (redDistance * redDistance) +
											 (greenDistance * greenDistance) +
											 (blueDistance * blueDistance);

						if (distance < leastDistance)
						{
							colorIndex = (byte) index;
							leastDistance = distance;

							// And if it's an exact match, exit the loop
							if (0 == distance)
								break;
						}
					}
				}

				// Now I have the color, pop it into the hashtable for next time
				colorMap.Add(colorHash, colorIndex);
			}

			return colorIndex;
		}

		/// <summary>
		/// Retrieve the palette for the quantized image
		/// </summary>
		/// <param name="palette">Any old palette, this is overrwritten</param>
		/// <returns>The new color palette</returns>
		protected override ColorPalette GetPalette(ColorPalette palette)
		{
			for (int index = 0; index < colors.Length; index++)
				palette.Entries[index] = colors[index];

			return palette;
		}
	}
}
