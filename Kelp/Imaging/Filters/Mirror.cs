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
	/// Implements a horizontal flip filter
	/// </summary>
	public class MirrorH : IFilter
	{
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
			source.RotateFlip(RotateFlipType.Rotate180FlipY);
			return source;
		}

		[QueryFilterFactory("mh", 1)]
		internal static IFilter GetMirrorHFilter(string[] param)
		{
			return param[0] == "1" ? new MirrorH() : null;
		}
	}

	/// <summary>
	/// Implements a vertical flip filter
	/// </summary>
	public class MirrorV : IFilter
	{
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
			source.RotateFlip(RotateFlipType.Rotate180FlipX);
			return source;
		}

		[QueryFilterFactory("mv", 1)]
		internal static IFilter GetMirrorVFilter(string[] param)
		{
			return param[0] == "1" ? new MirrorV() : null;
		}
	}
}
