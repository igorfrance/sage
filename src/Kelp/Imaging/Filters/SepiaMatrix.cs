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
