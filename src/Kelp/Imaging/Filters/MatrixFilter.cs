namespace Kelp.Imaging.Filters
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;

	/// <summary>
	/// Represents a filter that uses a matrix to modify a bitmap.
	/// </summary>
	public class MatrixFilter : IFilter
	{
		private IntRange range = new IntRange(-100, 100);

		/// <summary>
		/// Gets or sets the matrix associated with this <see cref="MatrixFilter"/>.
		/// </summary>
		/// <value>
		/// The matrix.
		/// </value>
		public float[][] Matrix { get; protected set; }

		/// <summary>
		/// Gets or sets the value range associated with this filter.
		/// </summary>
		public IntRange Range
		{
			get
			{
				return range;
			}

			protected set
			{
				range = value;
			}
		}

		/// <summary>
		/// Determines whether the specified <paramref name="format"/> is supported by the <see cref="MatrixFilter"/> 
		/// class
		/// </summary>
		/// <param name="format">The format to check.</param>
		/// <returns>
		/// <c>true</c> if the specified <paramref name="format"/> is supported by the <see cref="MatrixFilter"/> 
		/// class; otherwise, <c>false</c>.
		/// </returns>
		public static bool IsGraphicsSupported(PixelFormat format)
		{
			return
				format != PixelFormat.Undefined &&
				format != PixelFormat.DontCare &&
				format != PixelFormat.Format1bppIndexed &&
				format != PixelFormat.Format4bppIndexed &&
				format != PixelFormat.Format8bppIndexed &&
				format != PixelFormat.Format16bppGrayScale &&
				format != PixelFormat.Format16bppArgb1555;
		}

		/// <summary>
		/// Applies the current filter to the specified input <paramref name="source"/> bitmap and returns the result
		/// as a new bitmap.
		/// </summary>
		/// <param name="source">The source bitmap to filter.</param>
		/// <returns>The filtered bitmap.</returns>
		public Bitmap Apply(Bitmap source)
		{
			if (!IsGraphicsSupported(source.PixelFormat))
			{
				throw new ArgumentException(
					String.Format("PixelFormat {0} is not supported by the Graphics class", source.PixelFormat), "source");
			}

			Bitmap copy = AForge.Imaging.Image.Clone(source);

			ColorMatrix colorMatrix = new ColorMatrix(this.Matrix);
			ImageAttributes attributes = new ImageAttributes();
			attributes.SetColorMatrix(colorMatrix);

			Graphics g = Graphics.FromImage(copy);
			Rectangle destRect = new Rectangle(0, 0, source.Width, source.Height);
			g.DrawImage(source, destRect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel, attributes);
			g.Dispose();
			attributes.Dispose();

			return copy;
		}
	}
}
