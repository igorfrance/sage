namespace Kelp.Imaging
{
	using System;
	using System.Collections.Generic;
	using System.Drawing;

	/// <summary>
	/// Provides a container for a list of filters that can be applied as one.
	/// </summary>
	public class FilterSequence : List<IFilter>, IFilter
	{
		/// <summary>
		/// Applies all filters constituent in this filter sequence.
		/// </summary>
		/// <param name="source">The source bitmap to apply the filters on.</param>
		/// <returns>The new bitmap with filters applied</returns>
		public Bitmap Apply(Bitmap source)
		{
			Bitmap result = new Bitmap(source);
			foreach (IFilter filter in this)
				result = filter.Apply(result);

			return result;
		}
	}
}
