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
