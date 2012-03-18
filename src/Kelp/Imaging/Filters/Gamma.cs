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
	/// Matrix based gamma adjusting filter.
	/// </summary>
	public class GammaMatrix : MatrixFilter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GammaMatrix"/> class, setting the percentage to 20.
		/// </summary>
		public GammaMatrix()
			: this(20)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GammaMatrix"/> class with the specified 
		/// <paramref name="percent"/> brightness.
		/// </summary>
		/// <param name="percent">The percent of brightness this filter should apply.</param>
		public GammaMatrix(int percent)
		{
			percent = Range.GetValue(percent);

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

			this.Matrix = matrix;
		}
	}
}
