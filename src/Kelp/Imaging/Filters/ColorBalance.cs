namespace Kelp.Imaging.Filters
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;

	/// <summary>
	/// Implements a filter that manipulates the red, green and blue components of a bitmap.
	/// </summary>
	public class ColorBalance : IFilter
	{
		private IntRange rangeR = new IntRange(-255, 255);
		private IntRange rangeG = new IntRange(-255, 255);
		private IntRange rangeB = new IntRange(-255, 255);

		private int red, green, blue;

		/// <summary>
		/// Initializes a new instance of the <see cref="ColorBalance"/> class.
		/// </summary>
		public ColorBalance()
			: this(100, 0, 0)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ColorBalance"/> class.
		/// </summary>
		/// <param name="red">The red balance value.</param>
		/// <param name="green">The green balance value.</param>
		/// <param name="blue">The blue balance value.</param>
		public ColorBalance(int red, int green, int blue)
		{
			Red = red;
			Green = green;
			Blue = blue;
		}

		/// <summary>
		/// Gets or sets the red balance value of this filter.
		/// </summary>
		public int Red
		{
			get { return red; }
			set { red = rangeR.GetValue(value); }
		}

		/// <summary>
		/// Gets or sets the green balance value of this filter.
		/// </summary>
		public int Green
		{
			get { return green; }
			set { green = rangeG.GetValue(value); }
		}

		/// <summary>
		/// Gets or sets the blue balance value of this filter.
		/// </summary>
		public int Blue
		{
			get { return blue; }
			set { blue = rangeB.GetValue(value); }
		}

		/// <summary>
		/// Applies the color adjustment to the specified input <paramref name="source"/> bitmap and returns the result
		/// as a new bitmap.
		/// </summary>
		/// <param name="source">The source bitmap to filter.</param>
		/// <returns>The filtered bitmap.</returns>
		public Bitmap Apply(Bitmap source)
		{
			Bitmap copy = AForge.Imaging.Image.Clone(source);

			// GDI+ still lies to us - the return format is BGR, NOT RGB.
			BitmapData data = copy.LockBits(
				new Rectangle(0, 0, copy.Width, copy.Height),
					ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

			int stride = data.Stride;
			System.IntPtr scan0 = data.Scan0;

			unsafe
			{
				byte* p = (byte*) (void*) scan0;

				int offset = stride - (copy.Width * 3);

				for (int y = 0; y < copy.Height; ++y)
				{
					for (int x = 0; x < copy.Width; ++x)
					{
						int pixel = p[2] + this.Red;
						pixel = Math.Max(pixel, 0);
						p[2] = (byte) Math.Min(255, pixel);

						pixel = p[1] + Green;
						pixel = Math.Max(pixel, 0);
						p[1] = (byte) Math.Min(255, pixel);

						pixel = p[0] + Blue;
						pixel = Math.Max(pixel, 0);
						p[0] = (byte) Math.Min(255, pixel);

						p += 3;
					}

					p += offset;
				}
			}

			copy.UnlockBits(data);
			return copy;
		}
	}
}
