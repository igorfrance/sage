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

		[QueryFilterFactory("sp", 1)]
		internal static IFilter GetSharpenFilter(string[] param)
		{
			return param[0] == "1" ? new GaussianSharpen(0.9, 1) : null;
		}

		[QueryFilterFactory("sx", 1)]
		internal static IFilter GetSharpenExFilter(string[] param)
		{
			int amount;
			int.TryParse(param[0], out amount);

			if (amount > 0 && amount < 100)
				return new GaussianSharpen(amount * 0.025, 3);

			return GetSharpenFilter(param);
		}
	}
}
