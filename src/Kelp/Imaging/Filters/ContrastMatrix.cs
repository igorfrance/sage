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
	/// Matrix based contrast adjusting filter.
	/// </summary>
	public class ContrastMatrix : MatrixFilter
	{
		private const decimal Convert = 0.8M;

		/// <summary>
		/// Initializes a new instance of the <see cref="ContrastMatrix"/> class, setting the percentage to 20.
		/// </summary>
		public ContrastMatrix()
			: this(20)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ContrastMatrix"/> class with the specified 
		/// <paramref name="percent"/> brightness.
		/// </summary>
		/// <param name="percent">The percent of brightness this filter should apply.</param>
		public ContrastMatrix(int percent)
		{
			percent = Range.GetValue(percent, Convert);

			float v = percent > 0
				? (float) Math.Pow(0.0195F * percent, 2)
				: percent * 0.009F;

			float scale = 1 + v;
			float offset = v / 2;

			float[][] matrix =
			{
				new float[] { scale, 0, 0, 0, 0 }, 
				new float[] { 0, scale, 0, 0, 0 }, 
				new float[] { 0, 0, scale, 0, 0 }, 
				new float[] { 0, 0, 0, 1, 0 }, 
				new float[] { -offset, -offset, -offset, 0, 1 }
			};

			this.Matrix = matrix;
		}
	}
}
