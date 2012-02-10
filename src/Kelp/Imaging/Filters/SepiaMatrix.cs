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
	/// Matrix based sepia adjusting filter.
	/// </summary>
	public class SepiaMatrix : MatrixFilter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="SepiaMatrix"/> class.
		/// </summary>
		public SepiaMatrix()
		{
			// setup the matrix, sepia is grayscale with a slight 
			// offset in the red, green and blue colors
			float[][] sepia =
			{ 
				new float[] { ImageHelper.GrayRed,   ImageHelper.GrayRed,    ImageHelper.GrayRed,   0, 0 }, 
				new float[] { ImageHelper.GrayGreen, ImageHelper.GrayGreen,  ImageHelper.GrayGreen, 0, 0 }, 
				new float[] { ImageHelper.GrayBlue,  ImageHelper.GrayBlue,   ImageHelper.GrayBlue,  0, 0 }, 
				new float[] { 0,                     0,                      0,                     1, 0 }, 
				new float[] { ImageHelper.SepiaRed,  ImageHelper.SepiaGreen, ImageHelper.SepiaBlue, 0, 1 }
			};

			this.Matrix = sepia;
		}
	}
}
