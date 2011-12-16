namespace Kelp.Imaging
{
	using System;
	using System.Drawing;
	using System.Drawing.Imaging;
	using System.Runtime.InteropServices;

	/// <summary>
	/// Abstract class for other quantizers.
	/// </summary>
	public unsafe abstract class Quantizer
	{
		private readonly bool singlePass;

		/// <summary>
		/// Initializes a new instance of the <see cref="Quantizer"/> class.
		/// </summary>
		/// <param name="singlePass">If true, the quantization only needs to loop through the source pixels once</param>
		/// <remarks>
		/// If you construct this class with a true value for singlePass, then the code will, when quantizing your image,
		/// only call the 'QuantizeImage' function. If two passes are required, the code will call 'InitialQuantizeImage'
		/// and then 'QuantizeImage'.
		/// </remarks>
		protected Quantizer(bool singlePass)
		{
			this.singlePass = singlePass;
		}

		/// <summary>
		/// Quantize an image and return the resulting output bitmap
		/// </summary>
		/// <param name="source">The image to quantize</param>
		/// <returns>A quantized version of the image</returns>
		public Bitmap Quantize(System.Drawing.Image source)
		{
			int height = source.Height;
			int width = source.Width;

			Rectangle bounds = new Rectangle(0, 0, width, height);
			Bitmap copy = new Bitmap(width, height, PixelFormat.Format32bppArgb);
			Bitmap output = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

			using (Graphics g = Graphics.FromImage(copy))
			{
				g.PageUnit = GraphicsUnit.Pixel;
				g.DrawImage(source, bounds);

				// this line would produce an image that was too small
				// g.DrawImageUnscaled(source, bounds);
			}

			BitmapData sourceData = null;

			try
			{
				sourceData = copy.LockBits(bounds, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

				if (!singlePass)
					FirstPass(sourceData, width, height);

				output.Palette = this.GetPalette(output.Palette);
				SecondPass(sourceData, output, width, height, bounds);
			}
			finally
			{
				copy.UnlockBits(sourceData);
			}

			return output;
		}

		/// <summary>
		/// Execute the first pass through the pixels in the image
		/// </summary>
		/// <param name="sourceData">The source data</param>
		/// <param name="width">The width in pixels of the image</param>
		/// <param name="height">The height in pixels of the image</param>
		protected virtual void FirstPass(BitmapData sourceData, int width, int height)
		{
			// Define the source data pointers. The source row is a byte to
			// keep addition of the stride value easier (as this is in bytes)
			byte* sourceRow = (byte*) sourceData.Scan0.ToPointer();
			for (int row = 0; row < height; row++)
			{
				int* sourcePixel = (int*) sourceRow;

				for (int col = 0; col < width; col++, sourcePixel++)
					InitialQuantizePixel((Color32*) sourcePixel);

				sourceRow += sourceData.Stride;
			}
		}

		/// <summary>
		/// Execute a second pass through the bitmap
		/// </summary>
		/// <param name="sourceData">The source bitmap, locked into memory</param>
		/// <param name="output">The output bitmap</param>
		/// <param name="width">The width in pixels of the image</param>
		/// <param name="height">The height in pixels of the image</param>
		/// <param name="bounds">The bounding rectangle</param>
		protected virtual void SecondPass(BitmapData sourceData, Bitmap output, int width, int height, Rectangle bounds)
		{
			BitmapData outputData = null;

			try
			{
				// Lock the output bitmap into memory
				outputData = output.LockBits(bounds, ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);

				// Define the source data pointers. The source row is a byte to
				// keep addition of the stride value easier (as this is in bytes)
				byte* sourceRow = (byte*) sourceData.Scan0.ToPointer();
				int* sourcePixel = (int*) sourceRow;
				int* previousPixel = sourcePixel;

				// Now define the destination data pointers
				byte* destinationRow = (byte*) outputData.Scan0.ToPointer();
				byte* destinationPixel = destinationRow;

				// And convert the first pixel, so that I have values going into the loop
				byte pixelValue = QuantizePixel((Color32*) sourcePixel);

				// Assign the value of the first pixel
				*destinationPixel = pixelValue;

				for (int row = 0; row < height; row++)
				{
					sourcePixel = (int*) sourceRow;
					destinationPixel = destinationRow;

					for (int col = 0; col < width; col++, sourcePixel++, destinationPixel++)
					{
						if (*previousPixel != *sourcePixel)
						{
							pixelValue = QuantizePixel((Color32*) sourcePixel);
							previousPixel = sourcePixel;
						}

						*destinationPixel = pixelValue;
					}

					sourceRow += sourceData.Stride;
					destinationRow += outputData.Stride;
				}
			}
			finally
			{
				output.UnlockBits(outputData);
			}
		}

		/// <summary>
		/// Override this to process the pixel in the first pass of the algorithm
		/// </summary>
		/// <param name="pixel">The pixel to quantize</param>
		/// <remarks>
		/// This function need only be overridden if your quantize algorithm needs two passes,
		/// such as an Octree quantizer.
		/// </remarks>
		protected virtual void InitialQuantizePixel(Color32* pixel)
		{
		}

		/// <summary>
		/// Override this to process the pixel in the second pass of the algorithm
		/// </summary>
		/// <param name="pixel">The pixel to quantize</param>
		/// <returns>The quantized value</returns>
		protected abstract byte QuantizePixel(Color32* pixel);

		/// <summary>
		/// Retrieve the palette for the quantized image
		/// </summary>
		/// <param name="original">Any old palette, this is overrwritten</param>
		/// <returns>The new color palette</returns>
		protected abstract ColorPalette GetPalette(ColorPalette original);

		/// <summary>
		/// Struct that defines a 32 bpp colour
		/// </summary>
		/// <remarks>
		/// This struct is used to read data from a 32 bits per pixel image
		/// in memory, and is ordered in this manner as this is the way that
		/// the data is layed out in memory 
		/// </remarks>
		[StructLayout(LayoutKind.Explicit)]
		public struct Color32
		{
			/// <summary>
			/// Holds the blue component of the colour
			/// </summary>
			[FieldOffset(0)]
			public byte Blue;

			/// <summary>
			/// Holds the green component of the colour
			/// </summary>
			[FieldOffset(1)]
			public byte Green;

			/// <summary>
			/// Holds the red component of the colour
			/// </summary>
			[FieldOffset(2)]
			public byte Red;

			/// <summary>
			/// Holds the alpha component of the colour
			/// </summary>
			[FieldOffset(3)]
			public byte Alpha;

			/// <summary>
			/// Permits the color32 to be treated as an int32
			/// </summary>
			[FieldOffset(0)]
			public int ARGB;

			/// <summary>
			/// Gets the color for this Color32 object
			/// </summary>
			public Color Color
			{
				get { return Color.FromArgb(Alpha, Red, Green, Blue); }
			}
		}
	}
}
