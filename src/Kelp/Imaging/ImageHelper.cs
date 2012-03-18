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
	using System.Drawing.Drawing2D;
	using System.Drawing.Imaging;

	/// <summary>
	/// Contains various utility and helper methods.
	/// </summary>
	public static class ImageHelper
	{
		/// <summary>
		/// Represents the gray blue color.
		/// </summary>
		public const float GrayBlue = 0.082F;

		/// <summary>
		/// Represents the gray green color.
		/// </summary>
		public const float GrayGreen = 0.6094F;
		
		/// <summary>
		/// Represents the gray red color.
		/// </summary>
		public const float GrayRed = 0.3086F;

		/// <summary>
		/// Represents the sepia blue color.
		/// </summary>
		public const float SepiaBlue = 0.08F;

		/// <summary>
		/// Represents the sepia green color.
		/// </summary>
		public const float SepiaGreen = 0.14F;

		/// <summary>
		/// Represents the sepia red color.
		/// </summary>
		public const float SepiaRed = 0.2F;

		/// <summary>
		/// Gets the default matrix for use with matrix-based filtering.
		/// </summary>
		public static float[][] DefaultMatrix
		{
			get
			{
				return new[] { 
					new float[] { 1, 0, 0, 0, 0 }, 
					new float[] { 0, 1, 0, 0, 0 }, 
					new float[] { 0, 0, 1, 0, 0 }, 
					new float[] { 0, 0, 0, 1, 0 }, 
					new float[] { 0, 0, 0, 0, 1 }
				};
			}
		}

		/// <summary>
		/// Gets the grayscale matrix for use with matrix-based filtering.
		/// </summary>
		public static float[][] GrayscaleMatrix
		{
			get
			{
				return new[] 
				{ 
					new float[] { GrayRed,   GrayRed,   GrayRed, 0, 0 }, 
					new float[] { GrayGreen, GrayGreen, GrayGreen, 0, 0 }, 
					new float[] { GrayBlue,  GrayBlue,  GrayBlue, 0, 0 }, 
					new float[] { 0, 0, 0, 1, 0 }, 
					new float[] { 0, 0, 0, 0, 1 },
				};
			}
		}

		/// <summary>
		/// Gets the sepia matrix for use with matrix-based filtering.
		/// </summary>
		public static float[][] SepiaMatrix
		{
			get
			{
				// sepia looks better if first darken the image, the matrix values 
				// (combination of brightness and sepia) could be hard coded, 
				// but leave in code to make it easy to modify
				float[][] bright = GetBrightnessMatrix(-25);

				// setup the matrix, sepia is grayscale with a slight 
				// offset in the red, green and blue colors
				float[][] sepia =
				{ 
					new float[] { GrayRed,   GrayRed,    GrayRed,   0, 0 }, 
					new float[] { GrayGreen, GrayGreen,  GrayGreen, 0, 0 }, 
					new float[] { GrayBlue,  GrayBlue,   GrayBlue,  0, 0 }, 
					new float[] { 0, 0, 0, 1, 0 }, 
					new float[] { SepiaRed,  SepiaGreen, SepiaBlue, 0, 1 }
				};

				return ImageHelper.CombineMatrixes(bright, sepia);
			}
		}

		/// <summary>
		/// Return the matrix for the specified brightness
		/// </summary>
		/// <param name="percent">Brightness amount</param>
		/// <returns>The matrix for the specified brightness</returns>
		public static float[][] GetBrightnessMatrix(int percent)
		{
			// set brightness by setting the offset row in the matrix
			// values can range from -1.0 (black) to 1.0 (white)
			// map the percent to use 60% of the full range (-0.6 to 0.6)
			float v = 0.005F * percent;

			// setup the matrix, sepia is grayscale with a slight 
			// offset in the red, green and blue colors
			float[][] matrix = 
			{
				new float[] { 1, 0, 0, 0, 0 }, 
				new float[] { 0, 1, 0, 0, 0 }, 
				new float[] { 0, 0, 1, 0, 0 }, 
				new float[] { 0, 0, 0, 1, 0 }, 
				new float[] { v, v, v, 0, 1 }
			};

			return matrix;
		}

		/// <summary>
		/// Return a matrix for the specified contrast
		/// </summary>
		/// <param name="percent">Contrast amount</param>
		/// <returns>The matrix for the specified contrast</returns>
		public static float[][] GetContrastMatrix(int percent)
		{
			float v = percent > 0
				? (float) Math.Pow(0.0195F * percent, 2)
				: percent * 0.009F;

			float scale = 1 + v;
			float offset = v / 2;

			float[][] matrix =
			{
				new float[] { scale, 0, 0, 0, 0 }, 
				new float[] { 0, scale, 0, 0, 0 }, 
				new float[] { 0, 0, scale, 0, 0 }, 
				new float[] { 0, 0, 0, 1, 0 }, 
				new float[] { -offset, -offset, -offset, 0, 1 }
			};
			return matrix;
		}

		/// <summary>
		/// Return a matrix for the specified saturation
		/// </summary>
		/// <param name="percent">Saturation amount</param>
		/// <returns>The matrix for the specified saturation</returns>
		public static float[][] GetSaturationMatrix(int percent)
		{
			float v = percent > 0
				? 1.0F + (0.015F * percent)
				: 1.0F - (0.009F * -percent);

			float r = (1.0F - v) * GrayRed;
			float g = (1.0F - v) * GrayGreen;
			float b = (1.0F - v) * GrayBlue;

			float[][] matrix =
			{
				new float[] { r + v, r, r, 0, 0 }, 
				new float[] { g, g + v, g, 0, 0 }, 
				new float[] { b, b, b + v, 0, 0 }, 
				new float[] { 0, 0, 0, 1, 0 }, 
				new float[] { 0, 0, 0, 0, 1 }
			};

			return matrix;
		}

		/// <summary>
		/// Adjusts the image brightness by the specified <paramref name="percent"/>.
		/// </summary>
		/// <param name="image">The image to process</param>
		/// <param name="percent">The percentage (-100 to 100) to adjust the brightness to</param>
		public static void AdjustBrightness(Bitmap image, int percent)
		{
			if (percent == 0)
				return;

			percent = percent < -100 ? -100 : percent > 100 ? 100 : percent;
			DrawImage(image, GetBrightnessMatrix(percent));
		}

		/// <summary>
		/// Adjusts the image contrast by the specified  <paramref name="percent"/>.
		/// </summary>
		/// <param name="image">The image to process</param>
		/// <param name="percent">The percentage (-100 to 100) to adjust the contrast to</param>
		public static void AdjustContrast(Bitmap image, int percent)
		{
			if (percent == 0)
				return;

			percent = percent < -100 ? -100 : percent > 100 ? 100 : percent;
			DrawImage(image, GetContrastMatrix(percent));
		}

		/// <summary>
		/// Adjusts the image saturation by the specified  <paramref name="percent"/>.
		/// </summary>
		/// <param name="image">The image to process</param>
		/// <param name="percent">The percentage (-100 to 100) to adjust the saturation to</param>
		public static void AdjustSaturation(Bitmap image, int percent)
		{
			if (percent == 0)
				return;

			percent = percent < -100 ? -100 : percent > 100 ? 100 : percent;
			DrawImage(image, GetSaturationMatrix(percent));
		}

		/// <summary>
		/// Adjusts the image gamma by the specified  <paramref name="percent"/>.
		/// </summary>
		/// <param name="image">The image to process</param>
		/// <param name="percent">The percentage of gamma (-100 to 100) to adjust the gamma to</param>
		public static void AdjustGamma(Bitmap image, int percent)
		{
			if (percent == 0)
				return;

			percent = percent < -100 ? -100 : percent > 100 ? 100 : percent;
			ImageAttributes attr = new ImageAttributes();

			//// the gamma value can range from 0.1 (bright) to 5.0 (dark)
			//// we allow the range of 0.15 to 3.5
			float gamma = percent > 0
				//// range from 1.0 (no change) to 0.15 (brighter)
				? 1.0F - (0.0085F * percent)
				//// range from 3.5 (darker) to 1.0 (no change)
				: 1.0F + (0.025F * -percent);

			try
			{
				attr.SetGamma(gamma);
				DrawImage(image, attr);
			}
			finally
			{
				attr.Dispose();
			}
		}

		/// <summary>
		/// Converts the image to grayscale
		/// </summary>
		/// <param name="image">The image to process</param>
		public static void ConvertToGrayscale(Bitmap image)
		{
			DrawImage(image, GrayscaleMatrix);
		}

		/// <summary>
		/// Converts the image to sepia
		/// </summary>
		/// <param name="image">The image to process</param>
		public static void ConvertToSepia(Bitmap image)
		{
			DrawImage(image, SepiaMatrix);
		}

		/// <summary>
		/// Adjusts the image using a custom matrix
		/// </summary>
		/// <param name="image">The image to process</param>
		/// <param name="matrix">The custom matrix</param>
		public static void AdjustUsingCustomMatrix(Bitmap image, float[][] matrix)
		{
			DrawImage(image, matrix);
		}

		/// <summary>
		/// Rotates the image using the built-in RotateFlip method
		/// </summary>
		/// <param name="image">The image to rotate</param>
		/// <param name="flipType">How to rotate the image</param>
		public static void Rotate(Bitmap image, RotateFlipType flipType)
		{
			image.RotateFlip(flipType);
		}

		/// <summary>
		/// Returns a part of the bitmap specified with a rectangle.
		/// </summary>
		/// <param name="image">The image to crop.</param>
		/// <param name="cropArea">Rectangle specifying the area to crop to.</param>
		/// <returns>The cropped image.</returns>
		public static Bitmap Crop(Bitmap image, Rectangle cropArea)
		{
			// create new image that will contain the cropped area
			Bitmap result = new Bitmap(cropArea.Width, cropArea.Height, image.PixelFormat);
			result.SetResolution(image.HorizontalResolution, image.VerticalResolution);
			Graphics graphics = Graphics.FromImage(result);

			try
			{
				// draw the cropped area onto the new image
				Rectangle sourceArea = new Rectangle(0, 0, result.Width, result.Height);
				graphics.DrawImage(image, sourceArea, cropArea, GraphicsUnit.Pixel);
			}
			finally
			{
				graphics.Dispose();
			}

			return result;
		}

		/// <summary>
		/// Returns a part of the bitmap as specified with the parameters that make up the rectangle.
		/// </summary>
		/// <param name="image">The image to crop.</param>
		/// <param name="x">The rectangle's top left coordinate's x component.</param>
		/// <param name="y">The rectangle's top left coordinate's y component.</param>
		/// <param name="width">The width of the rectangle.</param>
		/// <param name="height">The height of the rectangle.</param>
		/// <returns>The cropped image.</returns>
		public static Bitmap Crop(Bitmap image, int x, int y, int width, int height)
		{
			return Crop(image, new Rectangle(x, y, width, height));
		}

		/// <summary>
		/// Resizes the specified <paramref name="image"/> to the square calculated using the 
		/// <paramref name="longestSide"/> argument.
		/// </summary>
		/// <param name="image">The image to resize.</param>
		/// <param name="longestSide">The length of the longest side to adjust to</param>
		/// <returns>The resized image</returns>
		public static Bitmap Resize(Bitmap image, int longestSide)
		{
			// determine how to scale the image
			float scale = image.Width > image.Height
				? longestSide / (float) image.Width
				: longestSide / (float) image.Height;

			int width = (int) (image.Width * scale);
			int height = (int) (image.Height * scale);

			return Resize(image, width, height);
		}

		/// <summary>
		/// Resizes the specified <paramref name="image"/> to the specified <paramref name="width"/> and 
		/// <paramref name="height"/>
		/// </summary>
		/// <param name="image">The image to resize.</param>
		/// <param name="width">The width to resize to.</param>
		/// <param name="height">The height to resize to.</param>
		/// <returns>The resized image</returns>
		public static Bitmap Resize(Bitmap image, int width, int height)
		{
			if (width == image.Width && height == image.Height)
				return image;

			Bitmap newImage;

			try
			{
				// create the scaled image
				newImage = new Bitmap(width, height);

				// scale the image, the InterpolationMode specifies
				// the quality of the scaling operation
				Graphics g = Graphics.FromImage(newImage);
				Rectangle rect = new Rectangle(0, 0, width, height);

				g.InterpolationMode = InterpolationMode.HighQualityBicubic;
				g.DrawImage(image, rect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel);
				g.Dispose();
			}
			finally
			{
				// clean up the full size image
				image.Dispose();
			}

			return newImage;
		}

		/// <summary>
		/// Return a matrix that is the combination of the two specified matrixes,
		/// maintains the order information of the matrix
		/// </summary>
		/// <param name="m1">The first matrix to combine</param>
		/// <param name="m2">The second matrix to combine</param>
		/// <returns>Matrix that is the combination of the two specified matrixes</returns>
		public static float[][] CombineMatrixes(float[][] m1, float[][] m2)
		{
			float[][] matrix =
			{ 
				 new float[] { 0, 0, 0, 0, 0 }, 
				 new float[] { 0, 0, 0, 0, 0 }, 
				 new float[] { 0, 0, 0, 0, 0 }, 
				 new float[] { 0, 0, 0, 0, 0 }, 
				 new float[] { 0, 0, 0, 0, 0 }
			};

			for (int i = 0; i < 5; i++)
			{
				for (int j = 0; j < 5; j++)
				{
					matrix[j][i] =
					 (m1[j][0] * m2[0][i]) +
					 (m1[j][1] * m2[1][i]) +
					 (m1[j][2] * m2[2][i]) +
					 (m1[j][3] * m2[3][i]) +
					 (m1[j][4] * m2[4][i]);
				}
			}

			return matrix;
		}

		/// <summary>
		/// Returns the encoder whose description (JPEG, BMP...) equals the supplied fileFormat string
		/// </summary>
		/// <param name="fileFormat">The encoder to retrieve</param>
		/// <returns>The found encoder</returns>
		public static ImageCodecInfo GetEncoderInfo(string fileFormat)
		{
			fileFormat = fileFormat.ToUpper();

			ImageCodecInfo encoder = null;
			ImageCodecInfo[] encoderList = ImageCodecInfo.GetImageEncoders();

			for (int i = 0; i < encoderList.Length; i++)
			{
				if (encoderList[i].FormatDescription.Equals(fileFormat))
				{
					encoder = encoderList[i];
					break;
				}
			}

			return encoder;
		}

		/// <summary>
		/// Returns EncoderParametes populated with a single qiality 
		/// EncoderParameter, set to the supplied value.
		/// </summary>
		/// <param name="quality">The value of the quality encoder</param>
		/// <returns>The populated EncoderParameters</returns>
		public static EncoderParameters GetEncoderQualityParam(int quality)
		{
			EncoderParameters encParam = new EncoderParameters(1);
			EncoderParameter qualityRatio = new EncoderParameter(Encoder.Quality, quality);

			encParam.Param[0] = qualityRatio;
			return encParam;
		}

		/// <summary>
		/// Creates a new, 32-bit version of the supplied image
		/// </summary>
		/// <param name="image">The image to covert</param>
		/// <returns>The new, 32-bit version of the <paramref name="image"/></returns>
		public static Bitmap ConvertTo32bit(Bitmap image)
		{
			Bitmap newImage;

			if (IsGraphicsSupported(image.PixelFormat))
			{
				using (Graphics g1 = Graphics.FromImage(image))
				{
					newImage = new Bitmap(image.Width, image.Height, g1);
					using (Graphics g2 = Graphics.FromImage(newImage))
					{
						g2.DrawImage(image, 0, 0);
					}
				}
			}
			else
			{
				newImage = new Bitmap(image.Width, image.Height, PixelFormat.Format32bppRgb);
				using (Graphics g = Graphics.FromImage(newImage))
				{
					g.DrawImage(image, 0, 0);
				}
			}

			return newImage;
		}

		/// <summary>
		/// Creates a new, 8-bit version of the supplied image
		/// </summary>
		/// <param name="image">The image to covert</param>
		/// <returns>The new, 8-bit version of the <paramref name="image"/></returns>
		public static Bitmap ConvertTo8bit(Bitmap image)
		{
			PaletteQuantizer quantizer = new PaletteQuantizer(image);
			return quantizer.Quantize(image);
		}

		/// <summary>
		/// Creates a new, 8-bit version of the supplied image, and limiting the number of colors to
		/// <paramref name="maxColors"/>.
		/// </summary>
		/// <param name="image">The image to covert</param>
		/// <param name="maxColors">The maximum number of colors allowed in the resulting image.</param>
		/// <returns>The new, 8-bit version of the <paramref name="image"/></returns>
		public static Bitmap ConvertTo8bit(Bitmap image, byte maxColors)
		{
			OctreeQuantizer quantizer = new OctreeQuantizer(maxColors, 8);
			return quantizer.Quantize(image);
		}

		/// <summary>
		/// Returns a boolean indicating weather the supplied format is supported by the Graphics class
		/// </summary>
		/// <param name="format">The format to check</param>
		/// <returns>True is supported</returns>
		public static bool IsGraphicsSupported(PixelFormat format)
		{
			return
				format != PixelFormat.Undefined &&
				format != PixelFormat.DontCare &&
				format != PixelFormat.Format1bppIndexed &&
				format != PixelFormat.Format4bppIndexed &&
				format != PixelFormat.Format8bppIndexed &&
				format != PixelFormat.Format16bppGrayScale &&
				format != PixelFormat.Format16bppArgb1555;
		}

		/// <summary>
		/// Gets the codec info for the specified image <paramref name="format"/>.
		/// </summary>
		/// <param name="format">The format of the image for this to get the code.</param>
		/// <returns>The image codec info for the specified image <paramref name="format"/>.</returns>
		public static ImageCodecInfo GetCodecForType(ImageFormat format)
		{
			return GetCodecForType(MimeTypeFromImageFormat(format));
		}

		/// <summary>
		/// Gets the codec info for the specified <paramref name="mimeType"/>.
		/// </summary>
		/// <param name="mimeType">The mime-type of the image for this to get the code.</param>
		/// <returns>The image codec info for the specified <paramref name="mimeType"/>.</returns>
		public static ImageCodecInfo GetCodecForType(string mimeType)
		{
			if (mimeType.Equals("image/pjpeg"))
				mimeType = "image/jpeg";
			if (mimeType.Equals("image/x-png"))
				mimeType = "image/png";

			ImageCodecInfo[] imgEncoders = ImageCodecInfo.GetImageEncoders();

			for (int i = 0; i < imgEncoders.GetLength(0); i++)
				if (imgEncoders[i].MimeType == mimeType)
					return imgEncoders[i];

			return null;
		}

		/// <summary>
		/// Gets the list codecs supported on the current system.
		/// </summary>
		/// <returns>The list codecs supported on the current system.</returns>
		public static string[] GetSupportedCodecNames()
		{
			ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();

			int count = encoders.GetLength(0);

			string[] names = new string[count];
			for (int i = 0; i < count; i++)
				names[i] = encoders[i].MimeType;

			return names;
		}

		/// <summary>
		/// Gets the codecs supported on the current system joined by a linebreak in a single string.
		/// </summary>
		/// <returns>The codecs supported on the current system joined by a linebreak in a single string.</returns>
		public static string GetSupportedCodecString()
		{
			string[] names = GetSupportedCodecNames();
			System.Text.StringBuilder result = new System.Text.StringBuilder();
			for (int i = 0; i < names.Length; i++)
				result.AppendLine(names[i]);

			return result.ToString();
		}

		/// <summary>
		/// Draw the image using the specified matrix.
		/// </summary>
		/// <param name="image">The image ro process</param>
		/// <param name="matrix">The matrix to use</param>
		private static void DrawImage(Bitmap image, float[][] matrix)
		{
			ColorMatrix colorMatrix = new ColorMatrix(matrix);
			ImageAttributes attributes = new ImageAttributes();

			try
			{
				attributes.SetColorMatrix(colorMatrix);
				DrawImage(image, attributes);
			}
			finally
			{
				attributes.Dispose();
			}
		}

		/// <summary>
		/// Draw the image using the specified attributes
		/// </summary>
		/// <param name="image">The image to draw</param>
		/// <param name="attr">The attributes of the image</param>
		private static void DrawImage(Bitmap image, ImageAttributes attr)
		{
			if (!IsGraphicsSupported(image.PixelFormat))
			{
				throw new ArgumentException(
					String.Format("PixelFormat {0} is not supported by the Graphics class", image.PixelFormat), "image");
			}

			Graphics g = Graphics.FromImage(image);

			try
			{
				// draw the image using the attributes class
				Rectangle destRect = new Rectangle(0, 0, image.Width, image.Height);
				g.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, attr);
			}
			finally
			{
				g.Dispose();
			}
		}

		private static string MimeTypeFromImageFormat(ImageFormat format)
		{
			if (format.Equals(ImageFormat.Jpeg))
				return "image/jpeg";

			if (format.Equals(ImageFormat.Gif))
				return "image/gif";

			if (format.Equals(ImageFormat.Bmp))
				return "image/bmp";

			if (format.Equals(ImageFormat.Tiff))
				return "image/tiff";

			if (format.Equals(ImageFormat.Png))
				return "image/png";

			throw new ArgumentException("Unsupported  image format '" + format + "'", "format");
		}
	}
}
