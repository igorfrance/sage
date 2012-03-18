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
