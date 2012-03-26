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

	/// <summary>
	/// Matrix based grayscale filter.
	/// </summary>
	public class GrayscaleMatrix : MatrixFilter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GrayscaleMatrix"/> class.
		/// </summary>
		public GrayscaleMatrix()
		{
			this.Matrix = ImageHelper.GrayscaleMatrix;
		}

		[QueryFilterFactory("gs", 1)]
		internal static IFilter GetGrayscaleFilter(string[] param)
		{
			return param[0] == "1" ? new GrayscaleMatrix() : null;
		}
	}
}
