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
