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
	}
}
