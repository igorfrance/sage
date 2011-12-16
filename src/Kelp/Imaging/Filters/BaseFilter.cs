namespace Kelp.Imaging.Filters
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;

	/// <summary>
	/// Provides a base class for <see cref="IFilter"/> implementations.
	/// </summary>
	public abstract class BaseFilter : IFilter
	{
		/// <summary>
		/// Applies this filter to the specified input image.
		/// </summary>
		/// <param name="inputImage">The image to filter.</param>
		/// <returns>A new bitmap containing the filtered <paramref name="inputImage"/>.</returns>
		public abstract Bitmap Apply(Bitmap inputImage);

		/// <summary>
		/// Applies this filter to the specified input image data.
		/// </summary>
		/// <param name="imageData">The image data of the image to filter.</param>
		/// <returns>A new bitmap containing the filtered <paramref name="imageData"/>.</returns>
		public virtual Bitmap Apply(BitmapData imageData)
		{
			Bitmap result = new Bitmap(imageData.Width, imageData.Height, imageData.Stride, imageData.PixelFormat, imageData.Scan0);
			return Apply(result);
		}
	}
}
