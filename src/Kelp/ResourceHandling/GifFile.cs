namespace Kelp.ResourceHandling
{
	using System;
	using System.Collections.Generic;
	using System.Drawing.Imaging;
	using System.Linq;

	using Kelp.Imaging;

	/// <summary>
	/// Represents a GIF <see cref="ImageFile"/>
	/// </summary>
	public class GifFile : ImageFile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GifFile"/> class.
		/// </summary>
		/// <param name="absolutePath">The absolute path of the image.</param>
		public GifFile(string absolutePath)
			: base(absolutePath)
		{
		}

		/// <inheritdoc/>
		protected override ImageCodecInfo CodecInfo
		{
			get
			{
				return ImageHelper.GetCodecForType(ImageFormat.Gif);
			}
		}

		/// <inheritdoc/>
		protected override EncoderParameters CodecParameters
		{
			get
			{
				EncoderParameters encoderParams = new EncoderParameters(1);
				encoderParams.Param[0] = new EncoderParameter(Encoder.ColorDepth, 8);
				return encoderParams;
			}
		}
	}
}
