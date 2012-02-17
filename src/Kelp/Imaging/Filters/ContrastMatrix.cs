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
