/**
 * Open Source Initiative OSI - The MIT License (MIT):Licensing
 * [OSI Approved License]
 * The MIT License (MIT)
 *
 * Copyright (c) 2011 Igor France
 *
 * Permission is hereby granted, free of charge, to any person obtaining a
 * copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation
 * the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
 * THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
 * DEALINGS IN THE SOFTWARE.
 */
namespace Kelp.Imaging.Filters
{
	using System;
	using System.Drawing;
	using System.Drawing.Drawing2D;

	/// <summary>
	/// Defines the different fit types
	/// </summary>
	public enum ResampleFitType
	{
		/// <summary>
		/// Signifies that resampling should fit the image to within the maximum available dimensions.
		/// </summary>
		ToMaximums = 0,

		/// <summary>
		/// Signifies that resampling should fit the image to within the minimum available dimensions.
		/// </summary>
		ToMinimums = 1,
	}

	/// <summary>
	/// Implements a resample filter
	/// </summary>
	public class Resample : IFilter
	{
		private int width;
		private int height;

		/// <summary>
		/// Initializes a new instance of the <see cref="Resample"/> class.
		/// </summary>
		/// <param name="width">The target width of the image.</param>
		public Resample(int width)
		{
			this.FitType = ResampleFitType.ToMinimums;
			this.DontEnlarge = false;
			this.PreserveRatio = true;
			this.Interpolation = InterpolationMode.HighQualityBicubic;
			Width = width;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Resample"/> class.
		/// </summary>
		/// <param name="width">The target width of the image.</param>
		/// <param name="height">The target height of the image.</param>
		public Resample(int width, int height)
		{
			this.FitType = ResampleFitType.ToMinimums;
			this.DontEnlarge = false;
			this.PreserveRatio = true;
			this.Interpolation = InterpolationMode.HighQualityBicubic;
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Resample"/> class.
		/// </summary>
		/// <param name="width">The target width of the image.</param>
		/// <param name="height">The target height of the image.</param>
		/// <param name="interpolation">The interpolation mode to use for resampling.</param>
		public Resample(int width, int height, InterpolationMode interpolation)
			: this(width, height)
		{
			this.Interpolation = interpolation;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Resample"/> class.
		/// </summary>
		/// <param name="width">The target width of the image.</param>
		/// <param name="height">The target height of the image.</param>
		/// <param name="interpolation">The interpolation mode to use for resampling.</param>
		/// <param name="preserveRatio">if set to <c>true</c>, the resample will preserve the ration between dimensions.</param>
		public Resample(int width, int height, InterpolationMode interpolation, bool preserveRatio)
			: this(width, height, interpolation)
		{
			this.PreserveRatio = preserveRatio;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Resample"/> class.
		/// </summary>
		/// <param name="width">The target width of the image.</param>
		/// <param name="height">The target height of the image.</param>
		/// <param name="interpolation">The interpolation mode to use for resampling.</param>
		/// <param name="preserveRatio">if set to <c>true</c>, the resample will preserve the ration between dimensions.</param>
		/// <param name="dontEnlarge">if set to <c>true</c>, the image will not be enlarged.</param>
		public Resample(int width, int height, InterpolationMode interpolation, bool preserveRatio, bool dontEnlarge)
			: this(width, height, interpolation, preserveRatio)
		{
			this.DontEnlarge = dontEnlarge;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Resample"/> class.
		/// </summary>
		/// <param name="width">The target width of the image.</param>
		/// <param name="height">The target height of the image.</param>
		/// <param name="preserveRatio">if set to <c>true</c>, the resample will preserve the ration between dimensions.</param>
		/// <param name="dontEnlarge">if set to <c>true</c>, the image will not be enlarged.</param>
		public Resample(int width, int height, bool preserveRatio, bool dontEnlarge)
			: this(width, height)
		{
			this.PreserveRatio = preserveRatio;
			this.DontEnlarge = dontEnlarge;
		}

		/// <summary>
		/// Gets or sets the target width of the image.
		/// </summary>
		public int Width
		{
			get { return width; }
			set { width = Math.Max(0, Math.Min(5000, value)); }
		}

		/// <summary>
		/// Gets or sets the target width of the image.
		/// </summary>
		public int Height
		{
			get { return height; }
			set { height = Math.Max(0, Math.Min(5000, value)); }
		}

		/// <summary>
		/// Gets or sets the interpolation mode to use with resampling.
		/// </summary>
		public InterpolationMode Interpolation { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to preserve the ratio when resampling.
		/// </summary>
		public bool PreserveRatio
		 { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to disable enlargin of the image when resampling.
		/// </summary>
		public bool DontEnlarge
		 { get; set; }

		/// <summary>
		/// Gets or sets the fit type of the resample
		/// </summary>
		public ResampleFitType FitType
		 { get; set; }

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
			if (source == null)
				throw new ArgumentNullException("source");

			if (Width == 0 && Height == 0)
				return source;

			int w = this.Width;
			int h = this.Height;

			if (w == source.Width && h == source.Height)
				return source;

			if (this.DontEnlarge && (
					(w > source.Width && h > source.Height) ||
					(w > source.Width && h == 0) ||
					(h > source.Height && w == 0)
				))
				return source;

			if (h == 0)
				h = CalculateOtherSide(w, source.Width, source.Height);
			else if (w == 0)
				w = CalculateOtherSide(h, source.Height, source.Width);

			Bitmap newImage = null;

			try
			{
				if (this.PreserveRatio)
				{
					int diffW = source.Width - w;
					int diffH = source.Height - h;

					if (diffH > diffW)
					{
						if (this.FitType == ResampleFitType.ToMaximums)
							w = CalculateOtherSide(h, source.Height, source.Width);
						else
							h = CalculateOtherSide(w, source.Width, source.Height);
					}
					else
					{
						if (this.FitType == ResampleFitType.ToMaximums)
							h = CalculateOtherSide(w, source.Width, source.Height);
						else
							w = CalculateOtherSide(h, source.Height, source.Width);
					}

					if (this.FitType == ResampleFitType.ToMinimums)
					{
						if (h > Height && Height != 0)
						{
							h = Height;
							w = CalculateOtherSide(h, source.Height, source.Width);
						}

						if (w > Width && Width != 0)
						{
							w = Width;
							h = CalculateOtherSide(w, source.Width, source.Height);
						}
					}
				}

				// create the scaled image
				newImage = new Bitmap(w, h);

				// scale the image, the InterpolationMode specifies
				// the quality of the scaling operation
				Graphics g = Graphics.FromImage(newImage);
				Rectangle rect = new Rectangle(0, 0, w, h);

				g.InterpolationMode = this.Interpolation;
				g.DrawImage(source, rect, 0, 0, source.Width, source.Height, GraphicsUnit.Pixel);
				g.Dispose();
			}
			finally
			{
				// clean up the full size image
				source.Dispose();
			}

			return newImage;
		}

		private int CalculateOtherSide(int side1, int inputSide1, int inputSide2)
		{
			float ratio = inputSide1 / (float) side1;
			int side2 = (int) (inputSide2 / ratio);

			return side2;
		}
	}
}
