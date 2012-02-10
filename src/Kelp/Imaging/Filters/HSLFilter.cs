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
	using System.Collections.Generic;
	using System.Drawing;
	using System.Linq;
	using System.Text;

	/// <summary>
	/// Implements a HSL filter.
	/// </summary>
	public class HSLFilter : IFilter
	{
		private IntRange rangeH = new IntRange(-100, 100);
		private IntRange rangeS = new IntRange(-100, 100);
		private IntRange rangeL = new IntRange(-100, 100);

		private int amountH;
		private int amountS;
		private int amountL;

		private float convH = 1;
		private float convS = 1;
		private float convL = 1;

		/// <summary>
		/// Initializes a new instance of the <see cref="HSLFilter"/> class.
		/// </summary>
		public HSLFilter()
			: this(20, 20, 20)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="HSLFilter"/> class.
		/// </summary>
		/// <param name="hue">The hue value of the filter.</param>
		/// <param name="saturation">The saturation value of the filter.</param>
		/// <param name="lightness">The lightness value of the filter.</param>
		public HSLFilter(int hue, int saturation, int lightness)
		{
			this.Hue = hue;
			this.Saturation = saturation;
			this.Lightness = lightness;
		}

		/// <summary>
		/// Gets or sets the hue of the filter.
		/// </summary>
		public int Hue
		{
			get
			{
				return amountH;
			}

			set
			{
				amountH = rangeH.GetValue(value);
				if (amountH < 0)
				{
					convH = (float) (100 + amountH) * 0.01F;
				}
				else if (amountH > 0)
				{
					convH = 1F + (float) amountH * 0.07F;
				}
			}
		}

		/// <summary>
		/// Gets or sets the saturation of the filter.
		/// </summary>
		public int Saturation
		{
			get
			{
				return amountS;
			}

			set
			{
				amountS = rangeS.GetValue(value);
				if (amountS < 0)
				{
					convS = (float) (100 + amountS) * 0.01F;
				}
				else if (amountS > 0)
				{
					convS = 1F + (float) amountS * 0.07F;
				}
			}
		}

		/// <summary>
		/// Gets or sets the lightness of the filter.
		/// </summary>
		public int Lightness
		{
			get
			{
				return amountL;
			}

			set
			{
				amountL = rangeL.GetValue(value);
				if (amountL < 0)
				{
					convL = (float) (100 + amountL) * 0.01F;
				}
				else if (amountL > 0)
				{
					convL = 1F + (float) amountL * 0.07F;
				}
			}
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
			if (source == null)
				throw new ArgumentNullException("source");

			if (Hue == 0 && Saturation == 0 && Lightness == 0)
				return source;

			Bitmap result = ColorSpace.HSL(source, convH, convS, convL);
			return result;
		}
	}
}
