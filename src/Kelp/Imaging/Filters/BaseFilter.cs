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
namespace Kelp.Imaging.Filters
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;

	/// <summary>
	/// Provides a base class for <see cref="IFilter"/> implementations.
	/// </summary>
	public abstract class BaseFilter : IFilter
	{
		/// <summary>
		/// Applies this filter to the specified input image.
		/// </summary>
		/// <param name="inputImage">The image to filter.</param>
		/// <returns>A new bitmap containing the filtered <paramref name="inputImage"/>.</returns>
		public abstract Bitmap Apply(Bitmap inputImage);

		/// <summary>
		/// Applies this filter to the specified input image data.
		/// </summary>
		/// <param name="imageData">The image data of the image to filter.</param>
		/// <returns>A new bitmap containing the filtered <paramref name="imageData"/>.</returns>
		public virtual Bitmap Apply(BitmapData imageData)
		{
			Bitmap result = new Bitmap(imageData.Width, imageData.Height, imageData.Stride, imageData.PixelFormat, imageData.Scan0);
			return Apply(result);
		}
	}
}
