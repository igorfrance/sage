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
