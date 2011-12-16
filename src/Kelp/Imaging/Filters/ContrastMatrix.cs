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
