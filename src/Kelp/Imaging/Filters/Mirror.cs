namespace Kelp.Imaging.Filters
{
	using System;
	using System.Drawing;

	/// <summary>
	/// Implements a horizontal flip filter
	/// </summary>
	public class MirrorH : IFilter
	{
		/// <summary>
		/// Applies the filter to the specified input <paramref name="source"/> bitmap and returns the result
		/// as a new bitmap.
		/// </summary>
		/// <param name="source">The source bitmap to filter.</param>
		/// <returns>
		/// The filtered bitmap.
		/// </returns>
		public Bitmap Apply(Bitmap source)
		{
			source.RotateFlip(RotateFlipType.Rotate180FlipY);
			return source;
		}
	}

	/// <summary>
	/// Implements a vertical flip filter
	/// </summary>
	public class MirrorV : IFilter
	{
		/// <summary>
		/// Applies the filter to the specified input <paramref name="source"/> bitmap and returns the result
		/// as a new bitmap.
		/// </summary>
		/// <param name="source">The source bitmap to filter.</param>
		/// <returns>
		/// The filtered bitmap.
		/// </returns>
		public Bitmap Apply(Bitmap source)
		{
			source.RotateFlip(RotateFlipType.Rotate180FlipX);
			return source;
		}
	}
}
