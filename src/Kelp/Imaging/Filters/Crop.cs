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

	using Crop1 = AForge.Imaging.Filters.Crop;

	/// <summary>
	/// Implements a crop filter.
	/// </summary>
	/// <remarks>This class is just a wrapper around the AForge's crop filter.</remarks>
	public class Crop : IFilter
	{
		private readonly Crop1 crop;

		/// <summary>
		/// Initializes a new instance of the <see cref="Crop"/> class.
		/// </summary>
		/// <param name="rect">The rectangle to crop the images to.</param>
		public Crop(Rectangle rect)
		{
			crop = new Crop1(rect);
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
			return crop.Apply(source);
		}
	}
}
