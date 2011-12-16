namespace Kelp.Imaging
{
	using System;
	using System.Drawing;

	/// <summary>
	/// Represents a bitmap filter.
	/// </summary>
	public interface IFilter
	{
		/// <summary>
		/// Applies the filter to the specified input <paramref name="source"/> bitmap and returns the result
		/// as a new bitmap.
		/// </summary>
		/// <param name="source">The source bitmap to filter.</param>
		/// <returns>The filtered bitmap.</returns>
		Bitmap Apply(Bitmap source);
	}
}
