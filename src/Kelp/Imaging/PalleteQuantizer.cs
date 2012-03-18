/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
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
