/**
 * Copyright 2012 Igor France
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Kelp.Imaging.Filters
{
	using System;
	using System.Diagnostics.Contracts;
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
			get
			{ 
				return this.height; 
			}
			
			set 
			{ 
				this.height = Math.Max(0, Math.Min(5000, value)); 
			}
		}

		/// <summary>
		/// Gets or sets the interpolation mode to use with resampling.
		/// </summary>
		public InterpolationMode Interpolation { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to preserve the ratio when resampling.
		/// </summary>
		public bool PreserveRatio { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether to disable enlargin of the image when resampling.
		/// </summary>
		public bool DontEnlarge { get; set; }

		/// <summary>
		/// Gets or sets the fit type of the resample
		/// </summary>
		public ResampleFitType FitType { get; set; }

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
			if (this.Width == 0 && this.Height == 0)
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

			Bitmap newImage;

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
						if (h > this.Height && this.Height != 0)
						{
							h = this.Height;
							w = CalculateOtherSide(h, source.Height, source.Width);
						}

						if (w > this.Width && this.Width != 0)
						{
							w = this.Width;
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

		[QueryFilterFactory("rs", 1)]
		internal static IFilter GetResampleFilter(string[] param)
		{
			int width, height = 0;
			bool preserveRatio = true;
			bool dontEnlarge = true;
			InterpolationMode interpolation = InterpolationMode.HighQualityBicubic;
			ResampleFitType fitType = ResampleFitType.ToMaximums;

			int.TryParse(param[0], out width);

			if (param.Length > 1)
				int.TryParse(param[1], out height);

			if (param.Length > 2)
				preserveRatio = param[2] != "0";

			if (param.Length > 3)
				dontEnlarge = param[3] != "0";

			if (param.Length > 4)
			{
				interpolation = (InterpolationMode)
					Enum.Parse(typeof(InterpolationMode), param[4]);
			}

			if (param.Length > 5 && param[5] == "min")
			{
				fitType = ResampleFitType.ToMinimums;
			}

			if (width != 0 || height != 0)
			{
				Resample filter = new Resample(width, height, interpolation, preserveRatio, dontEnlarge);
				filter.FitType = fitType;

				return filter;
			}

			return null;
		}

		private static int CalculateOtherSide(int side1, int inputSide1, int inputSide2)
		{
			float ratio = inputSide1 / (float)side1;
			int side2 = (int)(inputSide2 / ratio);

			return side2;
		}
	}
}
