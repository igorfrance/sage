namespace Kelp.ResourceHandling
{
	using System;
	using System.Collections.Generic;
	using System.Drawing.Imaging;
	using System.Linq;

	using Kelp.Imaging;

	/// <summary>
	/// Represents a PNG <see cref="ImageFile"/>
	/// </summary>
	public class PngFile : ImageFile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PngFile"/> class.
		/// </summary>
		/// <param name="absolutePath">The absolute path of the image.</param>
		public PngFile(string absolutePath)
			: base(absolutePath)
		{
		}

		/// <inheritdoc/>
		protected override ImageCodecInfo CodecInfo
		{
			get
			{
				return ImageHelper.GetCodecForType(ImageFormat.Png);
			}
		}

		/// <inheritdoc/>
		protected override EncoderParameters CodecParameters
		{
			get
			{
				EncoderParameters encoderParams = new EncoderParameters(1);
				encoderParams.Param[0] = new EncoderParameter(Encoder.ColorDepth, 32);
				return encoderParams;
			}
		}
	}
}
