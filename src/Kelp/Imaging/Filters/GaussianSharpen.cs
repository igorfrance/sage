namespace Kelp.Imaging.Filters
{
	using System;
	using System.Drawing;

	using Sharpen = AForge.Imaging.Filters.GaussianSharpen;

	/// <summary>
	/// Implements a gaussian sharpen filter.
	/// </summary>
	/// <remarks>This class is just a wrapper around the AForge's GaussianSharpen filter.</remarks>
	public class GaussianSharpen : IFilter
	{
		private readonly Sharpen sharpen;

		/// <summary>
		/// Initializes a new instance of the <see cref="GaussianSharpen"/> class.
		/// </summary>
		public GaussianSharpen()
		{
			sharpen = new Sharpen();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GaussianSharpen"/> class.
		/// </summary>
		/// <param name="sigma">The sigma of the sharpness.</param>
		public GaussianSharpen(double sigma)
		{
			sharpen = new Sharpen(sigma);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GaussianSharpen"/> class.
		/// </summary>
		/// <param name="sigma">The sigma of the sharpness.</param>
		/// <param name="size">The size of the sharpness.</param>
		public GaussianSharpen(double sigma, int size)
		{
			sharpen = new Sharpen(sigma, size);
		}

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
			return sharpen.Apply(source);
		}
	}
}
