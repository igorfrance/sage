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

	using Sharpen = AForge.Imaging.Filters.GaussianSharpen;

	/// <summary>
	/// Implements a gaussian sharpen filter.
	/// </summary>
	/// <remarks>This class is just a wrapper around the AForge's GaussianSharpen filter.</remarks>
	public class GaussianSharpen : IFilter
	{
		private readonly Sharpen sharpen;

		/// <summary>
		/// Initializes a new instance of the <see cref="GaussianSharpen"/> class.
		/// </summary>
		public GaussianSharpen()
		{
			sharpen = new Sharpen();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GaussianSharpen"/> class.
		/// </summary>
		/// <param name="sigma">The sigma of the sharpness.</param>
		public GaussianSharpen(double sigma)
		{
			sharpen = new Sharpen(sigma);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GaussianSharpen"/> class.
		/// </summary>
		/// <param name="sigma">The sigma of the sharpness.</param>
		/// <param name="size">The size of the sharpness.</param>
		public GaussianSharpen(double sigma, int size)
		{
			sharpen = new Sharpen(sigma, size);
		}

		/// <summary>
		/// Applies the filter to the specified input <paramref name="source"/> bitmap and returns the result
		/// as a new bitmap.
		/// </summary>
		/// <param name="source">The source bitmap to filter.</param>
		/// <returns>
		/// The filtered bitmap.
		/// </returns>
		public Bitmap Apply(Bitmap source)
		{
			return sharpen.Apply(source);
		}
	}
}
