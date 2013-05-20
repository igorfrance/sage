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
	using System.Drawing;
	using System.Drawing.Imaging;

	/// <summary>
	/// Provides utility methods for working with colors.
	/// </summary>
	internal static class ColorSpace
	{
		public static Bitmap HSL(Bitmap source, float factorH, float factorS, float factorL)
		{
			int width = source.Width;
			int height = source.Height;

			Rectangle rc = new Rectangle(0, 0, width, height);

			if (source.PixelFormat != PixelFormat.Format24bppRgb)
				source = source.Clone(rc, PixelFormat.Format24bppRgb);

			Bitmap dest = new Bitmap(width, height, source.PixelFormat);
			BitmapData dataSrc = source.LockBits(rc, ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
			BitmapData dataDest = dest.LockBits(rc, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int offset = dataSrc.Stride - (width * 3);

			unsafe
			{
				byte* bytesSrc = (byte*) (void*) dataSrc.Scan0;
				byte* bytesDest = (byte*) (void*) dataDest.Scan0;

				for (int y = 0; y < height; ++y)
				{
					for (int x = 0; x < width; ++x)
					{
						HSLValue hsl = HSLValue.FromRGB(bytesSrc[2], bytesSrc[1], bytesSrc[0]); // Still BGR
						hsl.Hue *= factorH;
						hsl.Saturation *= factorS;
						hsl.Luminance *= factorL;

						Color c = hsl.RGB;

						bytesDest[0] = c.B;
						bytesDest[1] = c.G;
						bytesDest[2] = c.R;

						bytesSrc += 3;
						bytesDest += 3;
					}

					bytesDest += offset;
					bytesSrc += offset;
				}

				source.UnlockBits(dataSrc);
				dest.UnlockBits(dataDest);
			}

			return dest;
		}

		public static Bitmap Hue(Bitmap source, float factor)
		{
			return HSL(source, factor, 1, 1);
		}

		public static Bitmap Saturation(Bitmap source, float factor)
		{
			return HSL(source, 1, factor, 1);
		}

		public static Bitmap Luminance(Bitmap source, float factor)
		{
			return HSL(source, 1, 1, factor);
		}
	}
}
