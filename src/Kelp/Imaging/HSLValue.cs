namespace Kelp.Imaging
{
	using System;
	using System.Drawing;

	/// <summary>
	/// Represents a HSL value.
	/// </summary>
	public class HSLValue
	{
		private float h;
		private float s;
		private float l;

		/// <summary>
		/// Initializes a new instance of the <see cref="HSLValue"/> class.
		/// </summary>
		/// <param name="hue">The hue value of the filter.</param>
		/// <param name="saturation">The saturation value of the filter.</param>
		/// <param name="luminance">The luminance value of the filter.</param>
		public HSLValue(float hue, float saturation, float luminance)
		{
			Hue = hue;
			Saturation = saturation;
			Luminance = luminance;
		}

		/// <summary>
		/// Gets or sets the hue of the filter.
		/// </summary>
		public float Hue
		{
			get { return h; }
			set { h = Math.Abs(value) % 360; }
		}

		/// <summary>
		/// Gets or sets the saturation of the filter.
		/// </summary>
		public float Saturation
		{
			get { return s; }
			set { s = (float) Math.Max(Math.Min(1.0, value), 0.0); }
		}

		/// <summary>
		/// Gets or sets the luminance of the filter.
		/// </summary>
		public float Luminance
		{
			get { return l; }
			set { l = (float) Math.Max(Math.Min(1.0, value), 0.0); }
		}

		/// <summary>
		/// Gets the RGB value corresponding to the HSL valus this instance represents.
		/// </summary>
		public Color RGB
		{
			get
			{
				double r, g, b;
				double normalisedH = h / 360.0;

				if (l == 0)
				{
					r = g = b = 0;
				}
				else
				{
					if (s == 0)
					{
						r = g = b = l;
					}
					else
					{
						double temp2 = (this.l <= 0.5) ? this.l * (1.0 + this.s) : this.l + this.s - (this.l * this.s);
						double temp1 = (2.0 * this.l) - temp2;

						double[] t3 = new double[] { normalisedH + (1.0 / 3.0), normalisedH, normalisedH - (1.0 / 3.0) };
						double[] clr = new double[] { 0, 0, 0 };

						for (int i = 0; i < 3; ++i)
						{
							if (t3[i] < 0)
								t3[i] += 1.0;

							if (t3[i] > 1)
								t3[i] -= 1.0;

							if (6.0 * t3[i] < 1.0)
								clr[i] = temp1 + ((temp2 - temp1) * t3[i] * 6.0);
							else if (2.0 * t3[i] < 1.0)
								clr[i] = temp2;
							else if (3.0 * t3[i] < 2.0)
								clr[i] = temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - t3[i]) * 6.0);
							else
								clr[i] = temp1;
						}

						r = clr[0];
						g = clr[1];
						b = clr[2];
					}
				}

				return Color.FromArgb((int) (255 * r), (int) (255 * g), (int) (255 * b));
			}
		}

		/// <summary>
		/// Gets a new <see cref="HSLValue"/> instance corresponding to the specified RGB values.
		/// </summary>
		/// <param name="red">The red component.</param>
		/// <param name="green">The green component.</param>
		/// <param name="blue">The blue component.</param>
		/// <returns>A new <see cref="HSLValue"/> instance corresponding to the specified RGB values</returns>
		public static HSLValue FromRGB(byte red, byte green, byte blue)
		{
			return FromRGB(Color.FromArgb(red, green, blue));
		}

		/// <summary>
		/// Gets a new <see cref="HSLValue"/> instance corresponding to the specified RGB color.
		/// </summary>
		/// <param name="color">The source RGB color ot use.</param>
		/// <returns>A new <see cref="HSLValue"/> instance corresponding to the specified RGB color.</returns>
		public static HSLValue FromRGB(Color color)
		{
			return new HSLValue(color.GetHue(), color.GetSaturation(), color.GetBrightness());
		}
	}
}
