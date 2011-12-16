namespace Kelp.ResourceHandling
{
	using System;
	using System.Drawing.Imaging;

	using Kelp.Imaging;

	/// <summary>
	/// Represents a JPEG <see cref="ImageFile"/>
	/// </summary>
	public class JpegFile : ImageFile
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JpegFile"/> class.
		/// </summary>
		/// <param name="absolutePath">The absolute path of the image.</param>
		public JpegFile(string absolutePath)
			: base(absolutePath)
		{
		}

		/// <inheritdoc/>
		protected override ImageCodecInfo CodecInfo
		{
			get
			{
				return ImageHelper.GetCodecForType(ImageFormat.Jpeg);
			}
		}

		/// <inheritdoc/>
		protected override EncoderParameters CodecParameters
		{
			get
			{
				EncoderParameters encoderParams = new EncoderParameters(1);
				encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, Compression);
				return encoderParams;
			}
		}

		private long Compression
		{
			get
			{
				long result = parameters.GetLong("Compression");
				if (result == 0)
					result = 90;

				return result;
			}
		}
	}
}
