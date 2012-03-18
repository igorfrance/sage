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
namespace Kelp.Imaging.Filters
{
	using System;
	using System.Drawing;

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
					convH = (100 + amountH) * 0.01F;
				}
				else if (amountH > 0)
				{
					convH = 1F + (amountH * 0.07F);
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
					convS = (100 + amountS) * 0.01F;
				}
				else if (amountS > 0)
				{
					convS = 1F + (amountS * 0.07F);
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
					convL = (100 + amountL) * 0.01F;
				}
				else if (amountL > 0)
				{
					convL = 1F + (amountL * 0.07F);
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
			if (this.Hue == 0 && this.Saturation == 0 && this.Lightness == 0)
				return source;

			Bitmap result = ColorSpace.HSL(source, this.convH, this.convS, this.convL);
			return result;
		}
	}
}
