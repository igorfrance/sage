namespace Kelp.Imaging.Filters
{
	using System;

	/// <summary>
	/// Matrix based brightness adjusting filter.
	/// </summary>
	public class BrightnessMatrix : MatrixFilter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="BrightnessMatrix"/> class, setting the percentage to 20.
		/// </summary>
		public BrightnessMatrix()
			: this(20)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="BrightnessMatrix"/> class with the specified 
		/// <paramref name="percent"/> brightness.
		/// </summary>
		/// <param name="percent">The percent of brightness this filter should apply.</param>
		public BrightnessMatrix(int percent)
		{
			percent = Range.GetValue(percent, 1.7M);

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
